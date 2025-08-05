string username = "jdoe"; // SAMAccountName
        DateTime latestLogon = DateTime.MinValue;

        // Get all Domain Controllers
        Domain domain = Domain.GetCurrentDomain();
        foreach (DomainController dc in domain.DomainControllers)
        {
            try
            {
                using (DirectoryEntry entry = new DirectoryEntry($"LDAP://{dc.Name}"))
                using (DirectorySearcher searcher = new DirectorySearcher(entry))
                {
                    searcher.Filter = $"(&(objectClass=user)(sAMAccountName={username}))";
                    searcher.PropertiesToLoad.Add("lastLogon");

                    SearchResult result = searcher.FindOne();
                    if (result != null && result.Properties.Contains("lastLogon"))
                    {
                        long lastLogonInt = (long)result.Properties["lastLogon"][0];
                        DateTime logonTime = DateTime.FromFileTimeUtc(lastLogonInt);

                        if (logonTime > latestLogon)
                            latestLogon = logonTime;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error querying {dc.Name}: {ex.Message}");
            }
        }

        Console.WriteLine($"Most recent lastLogon for {username}: {latestLogon}");
    }
