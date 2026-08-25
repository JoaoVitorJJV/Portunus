using Portunus.App.Domain.Util;
using Portunus.Core.Crypto;
using Portunus.Core.Crypto.DTO;
using Portunus.Core.DTO;
using Portunus.Core.Models;
using Portunus.Core.Vault;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Portunus.App.Services
{
    public class VaultService
    {
        private VaultSession? VaultSession { get; set;  }

        public bool IsVaultsCreated()
        {
            try
            {
                byte[] criptoVault = VaultStorage.Load(VaultLocation.DefaultPath);

                return criptoVault != null;
            }
            catch
            {
                return false;
            }
            
        }

        public void CreateVault(CreateVaultDTO createVaultDTO)
        {
            VaultSession vaultSession = VaultSession.CreateNew(createVaultDTO);
            VaultSession = vaultSession;
        }

        public bool UnlockVault(string masterPassword)
        {
            try
            {
                bool isUnlocked = VaultSession.TryUnlock(null, masterPassword, out VaultSession? vaultSession);

                if (!isUnlocked || vaultSession == null)
                    return false;

                VaultSession = vaultSession;

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void DisposeVault()
        {
            VaultSession?.Dispose();
            VaultSession = null;
        }

        public void DestroyVault()
        {
            VaultStorage.DeleteVault(VaultLocation.DefaultPath);
        }

        #region Get Itens
        public IReadOnlyList<PasswordEntry>? GetPasswordEntry()
        {
            if (VaultSession == null)
                return null;

            IReadOnlyList<PasswordEntry>? list = VaultSession.Entries;

            return list;
        }

        public IReadOnlyList<Vault>? GetVaults()
        {
            if (VaultSession == null)
                return null;

            IReadOnlyList<Vault>? list = VaultSession.Vaults;

            return list;
        }

        public IReadOnlyList<Category>? GetCategories()
        {
            if (VaultSession == null)
                return null;

            IReadOnlyList<Category>? list = VaultSession.Categories;

            return list;
        }
        #endregion

        #region Create/Update Items
        public Vault? SaveVault(Vault vault)
        {
            if (VaultSession == null)
                return null;

            VaultSession.SaveVault(vault);
            VaultSession.Save();

            return vault;
        }
        public Category? CreateNewCategory(Category category)
        {
            if (VaultSession == null)
                return null;

            VaultSession.SaveCategory(category);
            VaultSession.Save();

            return category;
        }

        public PasswordEntry? CreateUpdatePassword(PasswordEntry pass)
        {
            if (VaultSession == null)
                return null;

            VaultSession.SaveEntry(pass);
            VaultSession.Save();

            return pass;
        }
        #endregion

        #region Delete Items
        public void DeleteEntry(Guid id)
        {
            if(VaultSession == null)
                return;

            VaultSession.DeleteEntry(id);
            VaultSession.Save();
        }

        public void DeleteCategory(Guid id)
        {
            if (VaultSession == null)
                return;

            VaultSession.DeleteCategory(id);
            VaultSession.Save();
        }

        public void DeleteVault(Guid id)
        {
            if (VaultSession == null)
                return;

            VaultSession.DeleteVault(id);
            VaultSession.Save();
        }

        #endregion

    }
}
