using UnityEditor;

namespace Varwin.Editor
{
    public static class SdkTexts
    {
        public const string SdkDownloadHelpMessage = "How to update:\n"
                                                     + "1. Download the new version of Varwin SDK\n"
                                                     + "2. Delete folder: Assets/Varwin\n"
                                                     + "3. Import the downloaded Asset Package";

        public const string DisableAutoCheckToggle = "Disable Auto Check";

        public const string DownloadSdkButton = "Download Varwin SDK";

        public const string VersionsFormat = "Your version: {0}\nLatest version: {1}";

        public const string UpdateAvailableMessage = "A new version is available!";

        public const string NotNeedUpdateMessage = "You have the latest version";

        public const string CacheServerDisabledMobileSupportWarning =
            "Looks like your cache server is disabled. Switching it on can make building objects significantly faster.";

        public const string CacheServerSettingsInfo =
            "After turning the cache server on, you might want to choose a suitable directory for it and change its maximum size.";

        public const string CacheServerCheckWindowApplyButton = "Turn cache server on";

        public const string CacheServerWindowTitle = "Cache server config";
        
        public const string DontRemindMeAgain = "Don't remind me again";

        public const string SdkUpdateWindowTitle = "Varwin SDK Update";

        public const string SdkSettingsWindowTitle = "Varwin SDK Settings";

        public const string UnitySettingsWindowTitle = "Varwin Unity Project Settings";

        public const string CreateSceneTemplateWindowTitle = "Create scene template";

        public const string SceneTemplateWindowTitle = "Varwin Scene Template";

        public const string DefaultAuthorWindowTitle = "Default Author Info";

        public const string AboutWindowTitle = "About Varwin SDK";


        public const string ExperimentalFeatures = "Experimental Features";

        public const string DeveloperModeFeature = "Developer mode";
        
        public const string MobileBuildSupportFeature = "Mobile build support";

        public const string WebGLBuildSupportFeature = "WebGL build support";
        public const string LinuxBuildSupportFeature = "Linux build support";
        

        public const string OverrideDefaultObjectSettingsFeature = "Override default VarwinObjectDescriptor settings";
        public const string AddBehavioursAtRuntime = "Add behaviours at runtime";
        public const string MobileReady = "Mobile ready";
        public const string SourcesIncluded = "Sources included";
        public const string DisableSceneLogic = "Disable scene logic";

        public const string RecommendedProjectSettings = "Recommended project settings for Varwin:";

        public const string AllRecommendedOptionsWereApplied = "All recommended options were applied";



        public const string MoveCameraToEditorView = "Move camera to editor view";

        public const string TookScreenShotFormat = "Took screenshot to: {0}";

        public const string SceneTemplateSettings = "Scene Template Settings";

        public const string NoTeleportAreaMessage = "There are no objects with the tag \"TeleportArea\" at the scene. Continue building?";

        public const string NoTeleportAreaWarning = "There are no objects with the tag \"TeleportArea\" at the scene";

        public const string NoAndroidModuleWarning = "Android Module for Unity is not installed";

        public const string ResetDefaultAuthorSettingsButton = "Reset to default author settings";

        public const string AuthorNameEmptyWarning = "Author name can not be empty!";

        public const string SceneTemplateWasBuilt = "Scene template was built and packed.";

        public const string SceneTemplateBuildErrorMessage = "Something went wrong. Please check Unity console output for more info.";

        public const string SceneTemplateBuildStartMessage = "Starting create scene template...";


        public const string CannotCreateDirectoryTitle = "Can't create directory";

        public const string CannotCreateDirectoryFormat = "Can't create directory \"{0}\"";
        
        public const string CannotBuildSceneWithMissingReferencesFormat = "Can't build scene. Missing prefabs:\n{0}";
        
        public const string CannotDeleteFileFormat = "Can't delete file \"{0}\"";

        public const string CannotApplyPrefab = "Cannot apply changes in prefab";

        public const string BuildTargetNoSupportedFormat = "Build Target \"{0}\" is not supported";

        public const string CoreVersionFormat = "Core version is {0}";

        public const string ZipCreateSuccessMessage = "Zip was created!";

        public const string ZipCreateFailMessage = "Can not zip files!";


        public const string SaveDefaultAuthorInfoQuestion = "Save default author info changes?";

        public const string DefaultAuthorInfoWasUpdatedMessage = "Default author info have been updated.";

        public const string DefaultAuthorInfoWasReloadedMessage = "Default author info have been reloaded.";

        public const string DefaultAuthorInfoWillRevertQuestion = "Default author info will revert";

        public const string DefaultAuthorInfoWasRevertMessage = "Default author info have been reverted.";


        public const string CannotAutoGenerateWrapperFormat = "Wrapper {0} is not autogenerated! It would not be overwrite.";
        
        public const string ValueAttributeWithoutValueListFormat = "Value attribute without the additional ValueList attribute found for {0}.";
        
        public const string InconsistentValueListObjectTypesFormat = "ValueList \"{0}\" values have different types!";
        
        public const string PropertyLocaleAttributeMustHaveString = "Property locale attribute must have 1 string";
        
        public const string ValueIsPrivateFormat = "No public setter or getter found for the [Variable] \"{0}\"";
        
        public const string GetterIsPrivateFormat = "No public getter found for the [Getter] attribute for \"{0}\"";
        
        public const string SetterIsPrivateFormat = "No public setter found for the [Setter] attribute for \"{0}\"";
        
        public const string ObserveIsPrivateFormat = "No public setter or getter found for the [Observe] \"{0}\"";

        public const string ObserveMethodParametersNotSupported = "Not all parameters of method \"{0}\" are suitable for network syncronization. Please, remove an observe attribute or use one of the supported types.";
        
        public const string EnumIsPrivateFormat = "Enum \"{0}\" is private and can't be used with Actions, Functions and Checkers";
        
        public const string ArgsFormatNumberOfArgumentsMismatchFormat = "Number of arguments for \"{0}\" [ArgsFormat] attribute doesn't match the number of its corresponding parameters";

        public const string MethodMustReturnBoolFormat = "Method {0} must return bool!";

        public const string CountArgumentsMethodsIsNotEquals = "Count of arguments for methods with same actions is not equals";
        
        public const string ArgsFormatIsNotEqualsFormat = "Args format of the block {0} is not equals. C# method: {1}";

        public const string ArgsFormatInTypeIsNotEquals = "Args format in object {0} is not equals. Check console for details.";

        public const string CannotBuildActionArgumentMismatchFormat = "Can't build {0}. Actions with the same name must have an identical argument count and types";
        
        public const string CustomTypesForbiddenFormat = "{0}: It is forbidden to use custom types in methods, properties and events.";
        
        public const string EventAttributeNotVoidHandlerFormat = "Event \"{0}\" has non-void delegate return type which is not currently supported.";
        
        public const string MethodShouldReturnValueFormat = "Method \"{0}\" with the [Function]-attribute should return a value.";
        
        public const string ActionShouldReturnValueFormat = "Method \"{0}\" with the [Action]-attribute should return void or IEnumerator.";

        public const string WaitUntilObjectsCreate = "Please wait until all objects are built";

        public const string ObjectClassNameEmptyWarning = "Object Class Name can't be empty";

        public const string ObjectClassNameUnavailableSymbolsWarning = "Object Class Name contains invalid characters";

        public const string ObjectClassNameDuplicateWarning = "An object with the same Object Class Name already exists.";

        public const string ObjectNullWarning = "Object can't be null";

        public const string ObjectContainsComponentWarningFormat = "Object contains <{0}> component";

        public const string CannotCreateObjectErrorFormat = "{0}\nCan't create object";

        public const string ObjectWithNameAlreadyExists = "Object with this name already exists!";

        public const string ObjectWithNameAlreadyExistsFormat = "{0} already exists!";

        public const string PrefabAlreadyExistsWarningFormat = "{0} prefab already exists. Do you want to overwrite it?";

        public const string ConvertingToPrefabFormat = "Converting {0} to prefab.";

        public const string CannotCreateObjectWithoutPrefabAndModelFormat = "Can't create {0}: no prefab and no model. How did it happen? Please try again.";

        public const string CannotReadAuthorInfo = "cannot read author name and author url properties";

        public const string CannotReadLicenseInfo = "cannot read license code property";

        public const string TempBuildError = "Temp build file is broken! Can't finish objects creation.";

        public const string CreatingPrefabFormat = "Creating prefab for {0}";

        public const string CannotCreateObjectFormat = "Can't create object {0}. Imported file is incorrect: {1}";

        public const string ObjectsCreateDoneFormat = "{0} objects were created!";
        
        public const string SingleObjectCreateDoneFormat = "Object was created!";

        public const string ObjectsCreateProblemFormat = "{0}:\nProblem when creating objects";

        public const string NoSuitableForBuildObjectsFound = "No suitable for build objects found";

        public const string CompilingScriptsStep = "Compiling scripts...";

        public const string BuildingAssetBundlesStep = "Building asset bundles...";

        public const string PackSourcesStep = "Packing sources for {0}";

        public const string CreatePreviewStep = "Creating preview for {0}";

        public const string CreatingIconStepFormat = "Creating icon for {0}";

        public const string ProblemWhenRunBuildAllObjectsFormat = "{0} Problem when run build all objects";

        public const string ProblemWhenRunBuildVarwinObjectFormat = "{0}:\nProblem when run build varwin object \"{1}\"";

        public const string ProblemWhenBuildObjectsFormat = "{0}:\nProblem when build objects";

        public const string ProblemWhenBuildingAssetBundlesFormat = "{0}\nProblem when building asset bundles";
        
        public const string ProblemWhenPackSourcesFormat = "{0}\nProblem when pack sources";
        
        public const string ProblemWhenCreatePreview = "{0}\nProblem when create preview";
        
        public const string BuildTargetNotSupportFormat = "Build Target \"{0}\" is not supported";

        public const string UnityCompiling = "Unity is compiling. Please, wait...";
        
        public const string ScriptCompilationFailed = "Unity script compilation failed! All compiler errors have to be fixed before you can continue!";

        public const string MoveScriptQuestionFormat = "Are you sure you want to move the script \"{0}\" to the folder \"{1}\"?";
        public const string MoveScriptAndCreateAssemblyDefinitionQuestionFormat = "Are you sure you want to move the script \"{0}\" to the folder \"{1}\" and create an Assembly Definition in it?";
        
        public const string VarwinObjectBuildDialogTitle = "Varwin Object Build Dialog";
        public const string VarwinObjectLogicBuildDialogTitle = "Varwin Logic Build Dialog";

        public const string EditorWillProceed = "Editor will proceed to object building";
        public const string EditorLogicWillProceed = "Editor will proceed to logic building for selected objects";

        public const string ObjectTypeNameEmpty = "Object type name is empty";

        public const string ScriptWithoutAsmdefFormat = "The script \"{0}\" does not have Assembly Definition";
        
        public const string ScriptWithoutAsmdefInAssetsWithScriptsAsmdefFormat = "The script \"{0}\" does not have Assembly Definition. Move the script to the \"Assets/Scripts\" folder.";

        public const string ScriptWithoutAsmdefInAssetsFormat = "The script \"{0}\" does not have Assembly Definition. Move the script to the \"Assets/Scripts\" folder and create a Varwin Assembly Definition in it.";
        public const string ContinueSceneTemplateBuildingProblems = "If you continue building scene template now, it will work incorrectly.";
        

        public const string ObjectNotSelectable = "Object is not selectable. Add Rigidbody and Collider to the root object";
        public const string ObjectNotSelectableRigidbody = "Object is not selectable. Add Rigidbody to the root object";
        public const string ObjectNotSelectableCollider = "Object is not selectable. Add Collider to the root object";
        
        public const string SceneIsNotSaved = "You need to save the scene to build the scene template.";

        public const string DuplicateObjectIds = "Object contains duplicate Object Ids";

        public const string DuplicateObjectIdsHelp = "Object contains duplicate Object Ids. Remove duplicates?";

        public const string CreateNewVersion = "Create new version";

        public const string AnItemWithLanguageHasAlreadyBeenAddedFormat = "An item with the language \"{0}\" has already been added";


        public const string CreateNewVersionOfTheObjectMessage = "Create a new version of the object?";
        
        public const string CreateNewVersionOfTheObjectTitle = "Create a new version of the object";

        
        public const string VersionNotFoundTitle = "Varwin SDK version is not found";
            
        public const string VersionNotFoundMessage = "Varwin SDK version is not found! Please reinstall the SDK.";
        
        public const string SignaturesAreChanged = "{0} signatures have been changed since the previous build.";

        public const string EventGroupCustomSenderDifferentTitle = "Custom sender are different in same event group";

        public const string EventGroupCustomSenderDifferent = "Object {0} has different [CustomEventSender] attribute values is same event group {1}. " +
                                                              "Events in same event group must have same values in CustomEventSender attribute";

        public const string InspectorPropertyMissGetterOrSetterTitle = "Getter or setter is missing";
        public const string InspectorPropertyMissGetterOrSetter = "{0} missing in property {1} that use [VarwinInspector] attribute in {2} component";

        public const string SameLogicGroupDifferentBlockTypesTitle = "Block config creation error";
        
        public const string SameLogicGroupDifferentBlockTypesMessage = "Same name in logic block signature \"{0}\" detected. Please, change one of item's name";
        
        public const string DifferentActionGroupBlockReturnTypeTitle = "Block config creation error";
        public const string DifferentActionGroupBlockReturnTypeMessage = "Different return types is not allowed in action group. Group name: \"{0}\", Method name: \" {1}";
        public const string StandardShaderUsedWarning = "The object uses a standard shader. For better optimization, you need to use Varwin Standard shader";
        public const string PossibleDependencyOnOtherWarning = "Warning! An object may have dependencies on other Varwin objects.";
        public const string LinuxModulesMissingWarning = @"Building for Linux is not possible because the necessary module for building (Linux Build Support (Mono)) is not installed. For information on how to install modules, follow the link.";

        public const string WrongUnityVersionBuildMessage = "Can't build with current unity version {0}. Required unity version is {1}";
    }
}
