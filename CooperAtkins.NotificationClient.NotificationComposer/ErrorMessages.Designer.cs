﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CooperAtkins.NotificationClient.NotificationComposer {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class ErrorMessages {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal ErrorMessages() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("CooperAtkins.NotificationClient.NotificationComposer.ErrorMessages", typeof(ErrorMessages).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error: Missing entry in NotifyEmails/Groups for NotifyID= {0}.
        /// </summary>
        internal static string EmailComposer_EmailParamsNotSupplied {
            get {
                return ResourceManager.GetString("EmailComposer_EmailParamsNotSupplied", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error: Missing entry in GenStores.
        /// </summary>
        internal static string EmailComposer_GenStoreInvalid {
            get {
                return ResourceManager.GetString("EmailComposer_GenStoreInvalid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error:  Invalid email address format: {0} ...skipping.
        /// </summary>
        internal static string EmailComposer_InvalidEmailAddress {
            get {
                return ResourceManager.GetString("EmailComposer_InvalidEmailAddress", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error: Configure email parameters.
        /// </summary>
        internal static string EmailComposer_ParamsNotSupplied {
            get {
                return ResourceManager.GetString("EmailComposer_ParamsNotSupplied", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error: No SMTP Server Defined.
        /// </summary>
        internal static string EmailComposer_SMTPParmsNotSupplied {
            get {
                return ResourceManager.GetString("EmailComposer_SMTPParmsNotSupplied", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error: No SNPP Server Defined.
        /// </summary>
        internal static string PagerComposer_SNPPParmsNotSupplied {
            get {
                return ResourceManager.GetString("PagerComposer_SNPPParmsNotSupplied", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error:  Invalid SNPP pager address format: &quot; {0} &quot; ...skipping.
        /// </summary>
        internal static string PagerComposer_SNPPToAddressNotSupplied {
            get {
                return ResourceManager.GetString("PagerComposer_SNPPToAddressNotSupplied", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error: Custom script not found: {0}.
        /// </summary>
        internal static string ScriptComposer_InvalidScriptFileName {
            get {
                return ResourceManager.GetString("ScriptComposer_InvalidScriptFileName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error: Custom script not found: {0}.
        /// </summary>
        internal static string ScriptComposer_ScriptFileNotFound {
            get {
                return ResourceManager.GetString("ScriptComposer_ScriptFileNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error: Invalid settings: {0}.
        /// </summary>
        internal static string SMSComposer_InvalidSettings {
            get {
                return ResourceManager.GetString("SMSComposer_InvalidSettings", resourceCulture);
            }
        }
    }
}
