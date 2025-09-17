using JetBrains.Annotations;
using UnityEngine;

/// <summary>
/// Класс, показывающий пример использования уведомлений о шагах билда объекта.
/// </summary>
public class BuildStepNotifyObjectExample : MonoBehaviour
{
    #region references collecting state

    [UsedImplicitly]
    private void OnAsmdefReferencesCollectingStateEnter()
    {
        Debug.Log($"{gameObject}: {nameof(OnAsmdefReferencesCollectingStateEnter)}");
    }

    [UsedImplicitly]
    private void OnAsmdefReferencesCollectingStateExit()
    {
        Debug.Log($"{gameObject}: {nameof(OnAsmdefReferencesCollectingStateExit)}");
    }

    #endregion

    #region assemblies collecting state

    [UsedImplicitly]
    private void OnAssembliesCollectingStateEnter()
    {
        Debug.Log($"{gameObject}: {nameof(OnAssembliesCollectingStateEnter)}");
    }

    [UsedImplicitly]
    private void OnAssembliesCollectingStateExit()
    {
        Debug.Log($"{gameObject}: {nameof(OnAssembliesCollectingStateExit)}");
    }

    #endregion

    #region asset bundle building state

    [UsedImplicitly]
    private void OnAssetBundleBuildingStateEnter()
    {
        Debug.Log($"{gameObject}: {nameof(OnAssetBundleBuildingStateEnter)}");
    }

    [UsedImplicitly]
    private void OnAssetBundleBuildingStateExit()
    {
        Debug.Log($"{gameObject}: {nameof(OnAssetBundleBuildingStateExit)}");
    }

    #endregion
    
    #region bundle json generation state

    [UsedImplicitly]
    private void OnBundleJsonGenerationStateEnter()
    {
        Debug.Log($"{gameObject}: {nameof(OnBundleJsonGenerationStateEnter)}");
    }

    [UsedImplicitly]
    private void OnBundleJsonGenerationStateExit()
    {
        Debug.Log($"{gameObject}: {nameof(OnBundleJsonGenerationStateExit)}");
    }

    #endregion
    
    #region edit changelog state

    [UsedImplicitly]
    private void OnEditChangelogStateEnter()
    {
        Debug.Log($"{gameObject}: {nameof(OnEditChangelogStateEnter)}");
    }

    [UsedImplicitly]
    private void OnEditChangelogStateExit()
    {
        Debug.Log($"{gameObject}: {nameof(OnEditChangelogStateExit)}");
    }

    #endregion
    
    #region icon generation state

    [UsedImplicitly]
    private void OnIconGenerationStateEnter()
    {
        Debug.Log($"{gameObject}: {nameof(OnIconGenerationStateEnter)}");
    }

    [UsedImplicitly]
    private void OnIconGenerationStateExit()
    {
        Debug.Log($"{gameObject}: {nameof(OnIconGenerationStateExit)}");
    }

    [UsedImplicitly]
    private void OnIconGenerationStateWork(GameObject previewObject)
    {
        Debug.Log($"{gameObject}: {nameof(OnIconGenerationStateWork)}. Preview Object: {previewObject}");        
    }

    #endregion
    
    #region install json generation state

    [UsedImplicitly]
    private void OnInstallJsonGenerationStateEnter()
    {
        Debug.Log($"{gameObject}: {nameof(OnInstallJsonGenerationStateEnter)}");
    }

    [UsedImplicitly]
    private void OnInstallJsonGenerationStateExit()
    {
        Debug.Log($"{gameObject}: {nameof(OnInstallJsonGenerationStateExit)}");
    }

    #endregion

    #region object build validation state

    [UsedImplicitly]
    private void OnObjectsBuildValidationStateEnter()
    {
        Debug.Log($"{gameObject}: {nameof(OnObjectsBuildValidationStateEnter)}");
    }

    [UsedImplicitly]
    private void OnObjectsBuildValidationStateExit()
    {
        Debug.Log($"{gameObject}: {nameof(OnObjectsBuildValidationStateExit)}");
    }

    #endregion

    #region praparation state

    [UsedImplicitly]
    private void OnPreparationStateEnter()
    {
        Debug.Log($"{gameObject}: {nameof(OnPreparationStateEnter)}");
    }

    [UsedImplicitly]
    private void OnPreparationStateExit()
    {
        Debug.Log($"{gameObject}: {nameof(OnPreparationStateExit)}");
    }

    #endregion

    #region preview generation state

    [UsedImplicitly]
    private void OnPreviewGenerationStateEnter()
    {
        Debug.Log($"{gameObject}: {nameof(OnPreviewGenerationStateEnter)}");
    }

    [UsedImplicitly]
    private void OnPreviewGenerationStateExit()
    {
        Debug.Log($"{gameObject}: {nameof(OnPreviewGenerationStateExit)}");
    }

    [UsedImplicitly]
    private void OnPreviewGenerationStateWork(GameObject previewObject)
    {
        Debug.Log($"{gameObject}: {nameof(OnPreviewGenerationStateWork)}. Payload: {previewObject}");        
    }

    #endregion

    #region rename assemblies to new names state

    [UsedImplicitly]
    private void OnRenameAssembliesToNewNamesStateEnter()
    {
        Debug.Log($"{gameObject}: {nameof(OnRenameAssembliesToNewNamesStateEnter)}");
    }

    [UsedImplicitly]
    private void OnRenameAssembliesToNewNamesStateExit()
    {
        Debug.Log($"{gameObject}: {nameof(OnRenameAssembliesToNewNamesStateExit)}");
    }

    #endregion

    #region rename assemblies to old names state

    [UsedImplicitly]
    private void OnRenameAssembliesToOldNamesStateEnter()
    {
        Debug.Log($"{gameObject}: {nameof(OnRenameAssembliesToOldNamesStateEnter)}");
    }

    [UsedImplicitly]
    private void OnRenameAssembliesToOldNamesStateExit()
    {
        Debug.Log($"{gameObject}: {nameof(OnRenameAssembliesToOldNamesStateExit)}");
    }

    #endregion

    #region set version suffix to descriptor state

    [UsedImplicitly]
    private void OnSetVersionSuffixToDescriptorStateEnter()
    {
        Debug.Log($"{gameObject}: {nameof(OnSetVersionSuffixToDescriptorStateEnter)}");
    }

    [UsedImplicitly]
    private void OnSetVersionSuffixToDescriptorStateExit()
    {
        Debug.Log($"{gameObject}: {nameof(OnSetVersionSuffixToDescriptorStateExit)}");
    }

    #endregion

    #region source packing state

    [UsedImplicitly]
    private void OnSourcePackagingStateEnter()
    {
        Debug.Log($"{gameObject}: {nameof(OnSourcePackagingStateEnter)}");
    }

    [UsedImplicitly]
    private void OnSourcePackagingStateExit()
    {
        Debug.Log($"{gameObject}: {nameof(OnSourcePackagingStateExit)}");
    }

    #endregion

    #region wrapper generation state

    [UsedImplicitly]
    private void OnWrapperGenerationStateEnter()
    {
        Debug.Log($"{gameObject}: {nameof(OnWrapperGenerationStateEnter)}");
    }

    [UsedImplicitly]
    private void OnWrapperGenerationStateExit()
    {
        Debug.Log($"{gameObject}: {nameof(OnWrapperGenerationStateExit)}");
    }

    #endregion

    #region zipping files state

    [UsedImplicitly]
    private void OnZippingFilesStateEnter()
    {
        Debug.Log($"{gameObject}: {nameof(OnZippingFilesStateEnter)}");
    }

    [UsedImplicitly]
    private void OnZippingFilesStateExit()
    {
        Debug.Log($"{gameObject}: {nameof(OnZippingFilesStateExit)}");
    }

    #endregion
}