namespace iD365FOntTools.SecurityPrivilege
{
    using Microsoft.Dynamics.AX.Metadata.MetaModel;
    using Microsoft.Dynamics.Framework.Tools.Extensibility;
    using Microsoft.Dynamics.Framework.Tools.MetaModel.Automation.DataEntityViews;
    using Microsoft.Dynamics.Framework.Tools.MetaModel.Core;
    using System;
    using System.ComponentModel.Composition;

    /// <summary>
    /// Creates 2 Security privileges Maintain & View for the selected Menu item
    /// </summary>
    [Export(typeof(IMainMenu))]

    class SecurityPrivilegeCreatorMenuAddIn : MainMenuBase
    {
        #region Member variables
        private const string addinName = "iD365FOntTools.SecurityPrivilegeCreatorMenuAddIn";
        #endregion

        #region Properties
        /// <summary>
        /// Caption for the menu item. This is what users would see in the menu.
        /// </summary>
        public override string Caption
        {
            get
            {
                return AddinResources.SecurityPrivilegeCreatorMenuAddInCaption;
            }
        }

        /// <summary>
        /// Unique name of the add-in
        /// </summary>
        public override string Name
        {
            get
            {
                return SecurityPrivilegeCreatorMenuAddIn.addinName;
            }
        }
        #endregion

        #region Callbacks
        /// <summary>
        /// Called when user clicks on the add-in menu
        /// </summary>
        /// <param name="e">The context of the VS tools and metadata</param>
        public override void OnClick(AddinEventArgs e)
        {
            try
            {
                var vsProject = Common.CommonUtil.GetCurrentProject();

                EnvDTE.ProjectItems currentItems = Common.CommonUtil.GetCurrentProjectItems();

                AxSecurityPrivilege maintain = this.createMainSecurityElement(Common.Constants.MAINTAIN, vsProject.Name);
                AxSecurityPrivilege view = this.createMainSecurityElement(Common.Constants.VIEW, vsProject.Name);

                this.iterate(currentItems, maintain, view);
            }
            catch (Exception ex)
            {
                CoreUtility.HandleExceptionWithErrorMessage(ex);
            }
        }
        #endregion

        public void iterate(EnvDTE.ProjectItems currentItems, AxSecurityPrivilege maintain, AxSecurityPrivilege view)
        {
            Microsoft.Dynamics.AX.Metadata.Core.MetaModel.EntryPointType entryPointType;

            foreach (var item in currentItems)
            {
                if (item is Microsoft.Dynamics.Framework.Tools.ProjectSupport.Automation.OAFolderItem)
                {
                    Microsoft.Dynamics.Framework.Tools.ProjectSupport.Automation.OAFolderItem folder = item as Microsoft.Dynamics.Framework.Tools.ProjectSupport.Automation.OAFolderItem;
                    this.iterate(folder.ProjectItems, maintain, view);
                }
                else if (item is Microsoft.Dynamics.Framework.Tools.ProjectSystem.OAVSProjectFileItem)
                {
                    Microsoft.Dynamics.Framework.Tools.ProjectSystem.OAVSProjectFileItem current = item as Microsoft.Dynamics.Framework.Tools.ProjectSystem.OAVSProjectFileItem;
                    Microsoft.Dynamics.Framework.Tools.ProjectSystem.VSProjectFileNode currentObject = current.Object as Microsoft.Dynamics.Framework.Tools.ProjectSystem.VSProjectFileNode;
                    Microsoft.Dynamics.Framework.Tools.ProjectSystem.VSFileNodeProperties properties = currentObject.NodeProperties as Microsoft.Dynamics.Framework.Tools.ProjectSystem.VSFileNodeProperties;

                    if (properties.ItemType == "Menu Item Action"
                        || properties.ItemType == "Menu Item Output"
                        || properties.ItemType == "Menu Item Display")
                    {
                        entryPointType = Microsoft.Dynamics.AX.Metadata.Core.MetaModel.EntryPointType.MenuItemAction;
                        if (properties.ItemType == "Menu Item Action")
                        {
                            entryPointType = Microsoft.Dynamics.AX.Metadata.Core.MetaModel.EntryPointType.MenuItemAction;
                        }
                        else if (properties.ItemType == "Menu Item Display")
                        {
                            entryPointType = Microsoft.Dynamics.AX.Metadata.Core.MetaModel.EntryPointType.MenuItemDisplay;
                        }
                        else if (properties.ItemType == "Menu Item Output")
                        {
                            entryPointType = Microsoft.Dynamics.AX.Metadata.Core.MetaModel.EntryPointType.MenuItemOutput;
                        }

                        this.updateSecurityFromMenuItem(current, entryPointType, Common.Constants.MAINTAIN, maintain);
                        this.updateSecurityFromMenuItem(current, entryPointType, Common.Constants.VIEW, view);
                    }
                    else if (properties.ItemType == "Data Entity")
                    {
                        IDataEntityView selectedElementDataEntity = item as IDataEntityView;
                        this.updateSecurityFromDataEntity(current, Common.Constants.MAINTAIN, maintain);
                        this.updateSecurityFromDataEntity(current, Common.Constants.VIEW, view);
                    }
                    else if (properties.ItemType == "Table")
                    {
                        ITableBrowsable selectedElementTable = item as ITableBrowsable;
                        this.updateSecurityFromTable(current, Common.Constants.MAINTAIN, maintain);
                        this.updateSecurityFromTable(current, Common.Constants.VIEW, view);
                    }
                }
            }
        }

        private string updateSecurityFromMenuItem(
                Microsoft.Dynamics.Framework.Tools.ProjectSystem.OAVSProjectFileItem selectedMenuItem,
                Microsoft.Dynamics.AX.Metadata.Core.MetaModel.EntryPointType entryPointType,
                string suffix,
                AxSecurityPrivilege privilege)
        {

            AxSecurityPrivilege axSecurityPrivMaint = privilege;

            var entryPoint = new AxSecurityEntryPointReference()
            {
                ObjectType = entryPointType,
                ObjectName = selectedMenuItem.Name,
                Name = selectedMenuItem.Name,
                Grant = new Microsoft.Dynamics.AX.Metadata.Core.MetaModel.AccessGrant()
                {
                    Delete = suffix.Equals(Common.Constants.MAINTAIN)
                                                    ? Microsoft.Dynamics.AX.Metadata.Core.MetaModel.AccessGrantPermission.Allow
                                                    : Microsoft.Dynamics.AX.Metadata.Core.MetaModel.AccessGrantPermission.Unset,
                    Read = suffix.Equals(Common.Constants.VIEW)
                                                    ? Microsoft.Dynamics.AX.Metadata.Core.MetaModel.AccessGrantPermission.Allow
                                                    : Microsoft.Dynamics.AX.Metadata.Core.MetaModel.AccessGrantPermission.Unset
                }
            };
            entryPoint.Forms.Add(new AxSecurityEntryPointReferenceForm() { Name = selectedMenuItem.Name });
            axSecurityPrivMaint.EntryPoints.Add(entryPoint);

            // Find current model
            var modelSaveInfo = Common.CommonUtil.GetCurrentModelSaveInfo();

            var metaModelService = Common.CommonUtil.GetModelSaveService();
            metaModelService.CreateSecurityPrivilege(axSecurityPrivMaint, modelSaveInfo);

            // Assign the correct label on this by addint maintain or view in the label, copy the base label from the menu item           

            return axSecurityPrivMaint.Name;
        }

        private AxSecurityPrivilege updateSecurityFromDataEntity(
                Microsoft.Dynamics.Framework.Tools.ProjectSystem.OAVSProjectFileItem selectedDataEntity,
                string suffix,
                AxSecurityPrivilege privilege)
        {

            //Create Security privilege
            AxSecurityPrivilege axSecurityPriv = privilege;

            axSecurityPriv.DataEntityPermissions.Add(
                new AxSecurityDataEntityPermission()
                {
                    Grant = suffix == Common.Constants.MAINTAIN
                            ? Microsoft.Dynamics.AX.Metadata.Core.MetaModel.AccessGrant.ConstructGrantDelete()
                            : Microsoft.Dynamics.AX.Metadata.Core.MetaModel.AccessGrant.ConstructGrantRead(),
                    Name = selectedDataEntity.Name
                });

            // Find current model
            var modelSaveInfo = Common.CommonUtil.GetCurrentModelSaveInfo();

            var metaModelService = Common.CommonUtil.GetModelSaveService();
            metaModelService.CreateSecurityPrivilege(axSecurityPriv, modelSaveInfo);

            return privilege;
        }

        private AxSecurityPrivilege updateSecurityFromTable(
                Microsoft.Dynamics.Framework.Tools.ProjectSystem.OAVSProjectFileItem selectedDataEntity,
                string suffix,
                AxSecurityPrivilege privilege)
        {

            //Create Security privilege
            AxSecurityPrivilege axSecurityPriv = privilege;

            axSecurityPriv.DirectAccessPermissions.Add(
                new AxSecurityDataEntityReference()
                {
                    Grant = suffix == Common.Constants.MAINTAIN
                            ? Microsoft.Dynamics.AX.Metadata.Core.MetaModel.AccessGrant.ConstructGrantDelete()
                            : Microsoft.Dynamics.AX.Metadata.Core.MetaModel.AccessGrant.ConstructGrantRead(),
                    Name = selectedDataEntity.Name
                });

            var modelSaveInfo = Common.CommonUtil.GetCurrentModelSaveInfo();

            var metaModelService = Common.CommonUtil.GetModelSaveService();
            metaModelService.CreateSecurityPrivilege(axSecurityPriv, modelSaveInfo);

            return privilege;
        }

        private AxSecurityPrivilege createMainSecurityElement(
                string suffix,
                string name)
        {

            AxSecurityPrivilege axSecurityPrivMaint = new AxSecurityPrivilege() { Name = name + suffix };


            // Assign the correct label on this by addint maintain or view in the label, copy the base label from the menu item
            string label = name;
            if (label.StartsWith("@"))
            {
                label = Labels.LabelHelper.FindLabelGlobally(label).LabelText;
            }

            if (suffix.Equals(Common.Constants.MAINTAIN))
            {
                label = "Maintain " + label;
            }
            else if (suffix.Equals(Common.Constants.VIEW))
            {
                label = "View " + label;
            }
            // Convert to camel case
            if (String.IsNullOrEmpty(label) == false)
            {
                char[] a = label.ToLowerInvariant().ToCharArray();
                a[0] = char.ToUpperInvariant(a[0]);
                label = new String(a);
            }
            axSecurityPrivMaint.Label = label;

            // Find current model
            var modelSaveInfo = Common.CommonUtil.GetCurrentModelSaveInfo();

            var metaModelService = Common.CommonUtil.GetModelSaveService();
            metaModelService.CreateSecurityPrivilege(axSecurityPrivMaint, modelSaveInfo);

            Common.CommonUtil.AddElementToProject(axSecurityPrivMaint);

            return axSecurityPrivMaint;
        }
    }
}
