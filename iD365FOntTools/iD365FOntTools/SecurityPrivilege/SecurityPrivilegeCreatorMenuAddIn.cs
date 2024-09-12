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

                // Ya no creamos privilegios generales aquí. Los crearemos por cada elemento en iterate
                this.iterate(currentItems, vsProject.Name);
            }
            catch (Exception ex)
            {
                CoreUtility.HandleExceptionWithErrorMessage(ex);
            }
        }
        #endregion

        public void iterate(EnvDTE.ProjectItems currentItems, string projectName)
        {
            Microsoft.Dynamics.AX.Metadata.Core.MetaModel.EntryPointType entryPointType;

            foreach (var item in currentItems)
            {
                if (item is Microsoft.Dynamics.Framework.Tools.ProjectSupport.Automation.OAFolderItem folder)
                {
                    this.iterate(folder.ProjectItems, projectName);
                }
                else if (item is Microsoft.Dynamics.Framework.Tools.ProjectSystem.OAVSProjectFileItem current)
                {
                    Microsoft.Dynamics.Framework.Tools.ProjectSystem.VSProjectFileNode currentObject = current.Object as Microsoft.Dynamics.Framework.Tools.ProjectSystem.VSProjectFileNode;
                    Microsoft.Dynamics.Framework.Tools.ProjectSystem.VSFileNodeProperties properties = currentObject.NodeProperties as Microsoft.Dynamics.Framework.Tools.ProjectSystem.VSFileNodeProperties;

                    if (properties.ItemType == "Menu Item Action" || properties.ItemType == "Menu Item Output" || properties.ItemType == "Menu Item Display")
                    {
                        entryPointType = GetEntryPointType(properties.ItemType);

                        // Crear un privilegio "Maintain" para este elemento

                        var maintainPrivilege = this.createMainSecurityElement(Common.Constants.MAINTAIN, current.Caption == "" ? current.Name : current.Caption);
                        this.updateSecurityFromMenuItem(current, entryPointType, Common.Constants.MAINTAIN, maintainPrivilege);

                        // Crear un privilegio "View" para este elemento
                        var viewPrivilege = this.createMainSecurityElement(Common.Constants.VIEW, current.Caption == "" ? current.Name : current.Caption);
                        this.updateSecurityFromMenuItem(current, entryPointType, Common.Constants.VIEW, viewPrivilege);
                    }
                }
            }
        }

        private Microsoft.Dynamics.AX.Metadata.Core.MetaModel.EntryPointType GetEntryPointType(string itemType)
        {
            switch (itemType)
            {
                case "Menu Item Action":
                    return Microsoft.Dynamics.AX.Metadata.Core.MetaModel.EntryPointType.MenuItemAction;
                case "Menu Item Display":
                    return Microsoft.Dynamics.AX.Metadata.Core.MetaModel.EntryPointType.MenuItemDisplay;
                case "Menu Item Output":
                    return Microsoft.Dynamics.AX.Metadata.Core.MetaModel.EntryPointType.MenuItemOutput;
                default:
                    throw new ArgumentException("Tipo de elemento no reconocido.");
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

            if(entryPointType == Microsoft.Dynamics.AX.Metadata.Core.MetaModel.EntryPointType.MenuItemDisplay)
            {
                entryPoint.Forms.Add(new AxSecurityEntryPointReferenceForm() { Name = selectedMenuItem.Name });                
            }

            axSecurityPrivMaint.EntryPoints.Add(entryPoint);

            // Find current model
            var modelSaveInfo = Common.CommonUtil.GetCurrentModelSaveInfo();

            var metaModelService = Common.CommonUtil.GetModelSaveService();
            metaModelService.CreateSecurityPrivilege(axSecurityPrivMaint, modelSaveInfo);

            // Assign the correct label on this by addint maintain or view in the label, copy the base label from the menu item           

            return axSecurityPrivMaint.Name;
        }

        //private AxSecurityPrivilege updateSecurityFromDataEntity(
        //        Microsoft.Dynamics.Framework.Tools.ProjectSystem.OAVSProjectFileItem selectedDataEntity,
        //        string suffix,
        //        AxSecurityPrivilege privilege)
        //{

        //    //Create Security privilege
        //    AxSecurityPrivilege axSecurityPriv = privilege;

        //    axSecurityPriv.DataEntityPermissions.Add(
        //        new AxSecurityDataEntityPermission()
        //        {
        //            Grant = suffix == Common.Constants.MAINTAIN
        //                    ? Microsoft.Dynamics.AX.Metadata.Core.MetaModel.AccessGrant.ConstructGrantDelete()
        //                    : Microsoft.Dynamics.AX.Metadata.Core.MetaModel.AccessGrant.ConstructGrantRead(),
        //            Name = selectedDataEntity.Name
        //        });

        //    // Find current model
        //    var modelSaveInfo = Common.CommonUtil.GetCurrentModelSaveInfo();

        //    var metaModelService = Common.CommonUtil.GetModelSaveService();
        //    metaModelService.CreateSecurityPrivilege(axSecurityPriv, modelSaveInfo);

        //    return privilege;
        //}

        //private AxSecurityPrivilege updateSecurityFromTable(
        //        Microsoft.Dynamics.Framework.Tools.ProjectSystem.OAVSProjectFileItem selectedDataEntity,
        //        string suffix,
        //        AxSecurityPrivilege privilege)
        //{

        //    //Create Security privilege
        //    AxSecurityPrivilege axSecurityPriv = privilege;

        //    axSecurityPriv.DirectAccessPermissions.Add(
        //        new AxSecurityDataEntityReference()
        //        {
        //            Grant = suffix == Common.Constants.MAINTAIN
        //                    ? Microsoft.Dynamics.AX.Metadata.Core.MetaModel.AccessGrant.ConstructGrantDelete()
        //                    : Microsoft.Dynamics.AX.Metadata.Core.MetaModel.AccessGrant.ConstructGrantRead(),
        //            Name = selectedDataEntity.Name
        //        });

        //    var modelSaveInfo = Common.CommonUtil.GetCurrentModelSaveInfo();

        //    var metaModelService = Common.CommonUtil.GetModelSaveService();
        //    metaModelService.CreateSecurityPrivilege(axSecurityPriv, modelSaveInfo);

        //    return privilege;
        //}

        private AxSecurityPrivilege createMainSecurityElement(string suffix, string elementName)
        {
            AxSecurityPrivilege axSecurityPrivMaint = new AxSecurityPrivilege() { Name = elementName + suffix };

            // Asignar la etiqueta correcta para cada elemento
            string label = elementName;
            if (label.StartsWith("@"))
            {
                label = Labels.LabelHelper.FindLabelGlobally(label).LabelText;
            }

            label = suffix.Equals(Common.Constants.MAINTAIN) ? "Maintain " + label : "View " + label;

            // Convertir a camel case
            if (!string.IsNullOrEmpty(label))
            {
                char[] a = label.ToLowerInvariant().ToCharArray();
                a[0] = char.ToUpperInvariant(a[0]);
                label = new string(a);
            }
            axSecurityPrivMaint.Label = label;

            // Encontrar el modelo actual
            var modelSaveInfo = Common.CommonUtil.GetCurrentModelSaveInfo();
            var metaModelService = Common.CommonUtil.GetModelSaveService();
            metaModelService.CreateSecurityPrivilege(axSecurityPrivMaint, modelSaveInfo);

            // Añadir el elemento al proyecto
            Common.CommonUtil.AddElementToProject(axSecurityPrivMaint);

            return axSecurityPrivMaint;
        }
    }
}
