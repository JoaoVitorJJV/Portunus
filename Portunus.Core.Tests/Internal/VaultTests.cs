using Portunus.Core.DTO;
using Portunus.Core.Models;
using Portunus.Core.Vault;
using System.Text.Json;

namespace Portunus.Core.Tests.Internal
{
    public class VaultTests
    {
        private const string MasterPassword = "Bananao123";

        [Fact]
        public void CreateNewVault_TryDecryptVault_ReturnsTrue()
        {
            // Arrange
            string masterPassword = MasterPassword;
            string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.vault");
            CreateVaultDTO dto = new() { MasterPassword = masterPassword, VaultName = "Pessoal", Path = path };

            try
            {
                // Act
                using VaultSession vault = VaultSession.CreateNew(dto);
                bool tryUnlock = VaultSession.TryUnlock(path, masterPassword, out VaultSession? vaultUnlocked);


                // Assert
                Assert.NotNull(vaultUnlocked);
                Assert.True(tryUnlock);

                vaultUnlocked.Dispose();
            } 
            finally
            {
                if(File.Exists(path))
                {
                    File.Delete(path);
                }
            }
           
        }

        [Fact]
        public void CreateNewVault_TryDecryptVaultWithWrongPassword_ReturnsFalse()
        {
            // Arrange
            string masterPassword = MasterPassword;
            string wrongPassword = "WrongPassword";
            string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.vault");
            CreateVaultDTO dto = new() { MasterPassword = masterPassword, VaultName = "Pessoal", Path = path };
            try
            {
                // Act
                using VaultSession vault = VaultSession.CreateNew(dto);
                bool tryUnlock = VaultSession.TryUnlock(path, wrongPassword, out VaultSession? vaultUnlocked);

                // Assert
                Assert.Null(vaultUnlocked);
                Assert.False(tryUnlock);
            }
            finally
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        [Fact]
        public void CreateNewVault_InsertData_CompareOriginalData()
        {
            // Arrange
            Models.Vault vault = MockVault();
            Category category = MockCategory();
            PasswordTag tag = MockPasswordTag();
            PasswordEntry entry = MockPasswordEntry(vault.Id, category.Id, [tag.Id]);
            string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.vault");

            try
            {
                CreateNewVault(path);

                using (VaultSession first = UnlockVaultTest(path) ?? throw new Exception("Session is null"))
                {
                    first.SaveVault(vault);
                    first.SaveCategory(category);
                    first.SaveTag(tag);
                    first.SaveEntry(entry);
                }

                using VaultSession reopened = UnlockVaultTest(path) ?? throw new Exception("Session is null");

                Models.Vault savedVault = reopened.Vaults.Single(v => v.Id == vault.Id);
                Category savedCategory = reopened.Categories.Single(c => c.Id == category.Id);
                PasswordTag savedTag = reopened.Tags.Single(t => t.Id == tag.Id);
                PasswordEntry savedEntry = reopened.Entries.Single(e => e.Id == entry.Id);

                Assert.Equal(vault.Name, savedVault.Name);
                Assert.Equal(category.Name, savedCategory.Name);
                Assert.Equal(tag.Name, savedTag.Name);

                Assert.Equal(JsonSerializer.Serialize(entry), JsonSerializer.Serialize(savedEntry));

                Assert.Contains(reopened.Tags, t => t.Id == entry.TagIds.Single());
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        [Fact]
        public void CreateData_ThenDelete_DataIsGoneAndReferencesCleaned()
        {
            // Arrange
            PasswordTag tag = MockPasswordTag();
            Guid seededVaultId;
            Guid entryId;
            Guid tagId = tag.Id;
            string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.vault");
            try
            {
                CreateNewVault(path);

                using (VaultSession first = UnlockVaultTest(path) ?? throw new Exception("Session is null"))
                {
                    seededVaultId = first.Vaults[0].Id;
                    PasswordEntry entry = MockPasswordEntry(seededVaultId, tagIds: [tagId]);
                    entryId = entry.Id;

                    first.SaveTag(tag);
                    first.SaveEntry(entry);

                    // Act 
                    first.DeleteTag(tagId);
                }

                // Assert 
                using VaultSession reopened = UnlockVaultTest(path) ?? throw new Exception("Session is null");

                Assert.DoesNotContain(reopened.Tags, t => t.Id == tagId);
                PasswordEntry survivingEntry = reopened.Entries.Single(e => e.Id == entryId);
                Assert.DoesNotContain(tagId, survivingEntry.TagIds);
                Assert.NotNull(survivingEntry);
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        [Fact]
        public void SaveEntry_AfterAnotherEntry_BothPersist()
        {
            Guid firstId;
            Guid secondId;
            string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.vault");
            try
            {
                CreateNewVault(path);
                // Act
                using (VaultSession first = UnlockVaultTest(path) ?? throw new Exception("Session is null"))
                {
                    Guid vaultId = first.Vaults[0].Id;

                    PasswordEntry entryA = MockPasswordEntry(vaultId);
                    PasswordEntry entryB = MockPasswordEntry(vaultId);
                    firstId = entryA.Id;
                    secondId = entryB.Id;

                    first.SaveEntry(entryA);
                    first.SaveEntry(entryB);   
                }

                using VaultSession reopened = UnlockVaultTest(path) ?? throw new Exception("Session is null");

                // Assert
                Assert.Equal(2, reopened.Entries.Count);
                Assert.Contains(reopened.Entries, e => e.Id == firstId);
                Assert.Contains(reopened.Entries, e => e.Id == secondId);
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        [Fact]
        public void TwoVaults_EachWithOwnEntries_StayIsolatedByVaultId()
        {
            Guid vaultAId;
            Guid vaultBId;
            string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.vault");
            try
            {
                CreateNewVault(path);

                using (VaultSession first = UnlockVaultTest(path) ?? throw new Exception("Session is null"))
                {
                    vaultAId = first.Vaults[0].Id;     

                    Models.Vault vaultB = MockVault();
                    vaultBId = vaultB.Id;
                    first.SaveVault(vaultB);

                    // duas entradas no A, uma no B
                    first.SaveEntry(MockPasswordEntry(vaultAId));
                    first.SaveEntry(MockPasswordEntry(vaultAId));
                    first.SaveEntry(MockPasswordEntry(vaultBId));
                }

                using VaultSession reopened = UnlockVaultTest(path) ?? throw new Exception("Session is null");

                // Assert — os dois cofres existem
                Assert.Equal(2, reopened.Vaults.Count);

                // e cada entrada caiu no cofre certo, filtrando por VaultId
                int entriesInA = reopened.Entries.Count(e => e.VaultId == vaultAId);
                int entriesInB = reopened.Entries.Count(e => e.VaultId == vaultBId);

                Assert.Equal(2, entriesInA);
                Assert.Equal(1, entriesInB);
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        [Fact]
        public void TryUnlock_WhenFileDoesNotExist_ThrowsFileNotFound()
        {
            // Arrange 
            string ghostPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.vault");

            // Act + Assert — o lançamento É o comportamento esperado
            Assert.Throws<FileNotFoundException>(
                () => VaultSession.TryUnlock(ghostPath, "qualquerSenha", out _));
        }

        #region Métodos Privados
        private void CreateNewVault(string? path)
        {
            CreateVaultDTO dto = new() { MasterPassword = MasterPassword, VaultName = "Pessoal", Path = path };
            using VaultSession vault = VaultSession.CreateNew(dto);

        }

        private VaultSession? UnlockVaultTest(string? path)
        {
            try
            {
                bool tryUnlock = VaultSession.TryUnlock(path, MasterPassword, out VaultSession? vault);
                return vault;
            } catch (Exception)
            {
                throw;
            }
            
        }

        private static PasswordEntry MockPasswordEntry(Guid vaultId, Guid? categoryId = null, List<Guid>? tagIds = null)
        {
            return new()
            {
                VaultId = vaultId,
                CategoryId = categoryId,
                TagIds = tagIds is null ? [] : [.. tagIds],
                RecoveryCodes = [new() { Code = "Recovery Test 1..." }],
                Notes = "Notes 1..",
                Username = "joao@gmail.com",
                Password = "bananinha123",
                Url = "url.com",
                Name = "Password Mock Test"
            };
        }

        private static PasswordTag MockPasswordTag()
        {
            return new()
            {
                Name = "Tag Mock Test",
                BadgeColor = "#3B82F6",
                Description = "Tag de teste"
            };
        }

        private static Category MockCategory()
        {
            return new()
            {
                Name = "Category Mock Test",
                BadgeColor = "#8B5CF6",
                Description = "Categoria de teste"
            };
        }

        private static Models.Vault MockVault()
        {
            return new()
            {
                Name = "Vault Mock Test",
                Description = "Cofre de teste",
                BadgeColor = "#F59E0B"
            };
        }

        #endregion

    }
}
