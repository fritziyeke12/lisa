using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;

class Program
{
    static void Main()
    {
        var users = new List<AccountInventory>();

        // Get all domain controllers
        var domain = Domain.GetCurrentDomain();
        var domainControllers = domain.DomainControllers;

        // Get all users in the domain (from any DC)
        using (DirectoryEntry root = new DirectoryEntry())
        using (DirectorySearcher searcher = new DirectorySearcher(root))
        {
            searcher.Filter = "(objectCategory=person)";
            searcher.PageSize = 1000; // Enable paging
            searcher.PropertiesToLoad.AddRange(new[]
            {
                "cn", "sAMAccountName", "userPrincipalName",
                "whenCreated", "pwdLastSet", "userAccountControl"
            });

            foreach (SearchResult result in searcher.FindAll())
            {
                try
                {
                    string dn = result.Path; // Distinguished name for LDAP binding
                    string name = result.Properties["cn"]?[0]?.ToString() ?? "";
                    string sam = result.Properties["sAMAccountName"]?[0]?.ToString() ?? "";
                    string upn = result.Properties["userPrincipalName"]?[0]?.ToString() ?? "";
                    DateTime whenCreated = result.Properties["whenCreated"].Count > 0 ?
                        (DateTime)result.Properties["whenCreated"][0] : DateTime.MinValue;
                    DateTime pwdLastSet = result.Properties["pwdLastSet"].Count > 0 ?
                        DateTime.FromFileTimeUtc((long)result.Properties["pwdLastSet"][0]) : DateTime.MinValue;
                    bool isEnabled = !IsAccountDisabled(result);

                    // Get most recent lastLogon across all DCs
                    DateTime mostRecentLogon = GetMostRecentLastLogon(sam, domainControllers);

                    users.Add(new AccountInventory
                    {
                        Name = name,
                        SamAccountName = sam,
                        UserPrincipalName = upn,
                        LastLogon = mostRecentLogon,
                        WhenCreated = whenCreated,
                        PasswordLastSet = pwdLastSet,
                        IsEnabled = isEnabled
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error processing user: " + ex.Message);
                }
            }
        }

        // Output summary
        foreach (var user in users)
        {
            Console.WriteLine($"{user.SamAccountName,-20} | {user.LastLogon} | Enabled: {user.IsEnabled}");
        }

        // Optional: use or return the 'users' list here
    }

    static DateTime GetMostRecentLastLogon(string samAccountName, DomainControllerCollection dcs)
    {
        DateTime latest = DateTime.MinValue;

        foreach (DomainController dc in dcs)
        {
            try
            {
                using (DirectoryEntry entry = new DirectoryEntry($"LDAP://{dc.Name}"))
                using (DirectorySearcher searcher = new DirectorySearcher(entry))
                {
                    searcher.Filter = $"(&(objectClass=user)(sAMAccountName={samAccountName}))";
                    searcher.PropertiesToLoad.Add("lastLogon");

                    SearchResult result = searcher.FindOne();
                    if (result != null && result.Properties["lastLogon"].Count > 0)
                    {
                        long fileTime = Convert.ToInt64(result.Properties["lastLogon"][0]);
                        DateTime logonTime = DateTime.FromFileTimeUtc(fileTime);
                        if (logonTime > latest)
                            latest = logonTime;
                    }
                }
            }
            catch
            {
                // Skip DC if it fails
            }
        }

        return latest;
    }

    static bool IsAccountDisabled(SearchResult user)
    {
        if (user.Properties.Contains("userAccountControl"))
        {
            int uac = (int)user.Properties["userAccountControl"][0];
            return (uac & 0x2) != 0; // ACCOUNTDISABLE flag
        }
        return false; // Default to enabled if unknown
    }
}
