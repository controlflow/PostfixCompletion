﻿using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using JetBrains.Annotations;
using JetBrains.Application.Settings;
using JetBrains.DataFlow;
using JetBrains.ReSharper.Psi;
using JetBrains.UI.Extensions.Commands;
using JetBrains.UI.Options;

namespace JetBrains.ReSharper.PostfixTemplates.Settings
{
  // todo: tabs for languages
  public sealed class PostfixOptionsViewModel
  {
    [NotNull] private readonly OptionsSettingsSmartContext mySettingsStore;

    public PostfixOptionsViewModel(
      [NotNull] Lifetime lifetime, [NotNull] OptionsSettingsSmartContext settingsStore)
    {
      mySettingsStore = settingsStore;

      Templates = new ObservableCollection<PostfixTemplateViewModel>();

      ShowPostfixTemplates = new Property<bool>(lifetime, "ShowPostfixTemplates");
      ShowStaticMembers = new Property<bool>(lifetime, "ShowStaticMembers");
      ShowEnumHelpers = new Property<bool>(lifetime, "ShowEnumHelpers");
      ShowLengthCountItems = new Property<bool>(lifetime, "ShowLengthCountItems");
      UseBracesForStatements = new Property<bool>(lifetime, "UseBracesForStatements");
      InvokeParameterInfo = new Property<bool>(lifetime, "InvokeParameterInfo");
      SearchVarOccurrences = new Property<bool>(lifetime, "SearchVarOccurrences");

      Reset = new DelegateCommand(ResetExecute);

      settingsStore.SetBinding(lifetime, PostfixTemplatesSettingsAccessor.ShowPostfixItems, ShowPostfixTemplates);
      settingsStore.SetBinding(lifetime, PostfixTemplatesSettingsAccessor.ShowStaticMethods, ShowStaticMembers);
      settingsStore.SetBinding(lifetime, PostfixTemplatesSettingsAccessor.ShowEnumHelpers, ShowEnumHelpers);
      settingsStore.SetBinding(lifetime, PostfixTemplatesSettingsAccessor.BracesForStatements, UseBracesForStatements);
      settingsStore.SetBinding(lifetime, PostfixTemplatesSettingsAccessor.InvokeParameterInfo, InvokeParameterInfo);
      settingsStore.SetBinding(lifetime, PostfixTemplatesSettingsAccessor.ShowLengthCountItems, ShowLengthCountItems);
      settingsStore.SetBinding(lifetime, PostfixTemplatesSettingsAccessor.SearchVarOccurrences, SearchVarOccurrences);

      FillTemplates();
    }

    // ReSharper disable once CollectionNeverQueried.Global
    [NotNull] public ObservableCollection<PostfixTemplateViewModel> Templates { get; private set; }

    [NotNull] public IProperty<bool> ShowPostfixTemplates { get; private set; }
    // todo: [R#] public IProperty<bool> ShowSourceTemplates { get; private set; }

    [NotNull] public IProperty<bool> ShowStaticMembers { get; private set; }

    // todo: [R#] drop this two, enable always:
    [NotNull] public IProperty<bool> ShowEnumHelpers { get; private set; }
    [NotNull] public IProperty<bool> ShowLengthCountItems { get; private set; }

    [NotNull] public IProperty<bool> UseBracesForStatements { get; private set; }
    [NotNull] public IProperty<bool> InvokeParameterInfo { get; private set; }
    [NotNull] public IProperty<bool> SearchVarOccurrences { get; private set; }

    [NotNull] public ICommand Reset { get; private set; }

    private void FillTemplates()
    {
      var templatesManagers = LanguageManager.Instance.GetServicesFromAll<IPostfixTemplatesManager>();

      // todo: PostfixTemplatesForLanguageViewModel
      foreach (var specificTemplatesManager in templatesManagers)
      {
        var availableTemplates = specificTemplatesManager.AvailableTemplates;

        foreach (var providerInfo in availableTemplates.OrderBy(info => info.Metadata.TemplateName))
        {
          Templates.Add(new PostfixTemplateViewModel(providerInfo, mySettingsStore));
        }
      }
    }

    private void ResetExecute()
    {
      var settings = mySettingsStore.GetKey<PostfixTemplatesSettings>(SettingsOptimization.OptimizeDefault);
      settings.DisabledProviders.SnapshotAndFreeze();

      foreach (var provider in settings.DisabledProviders.EnumIndexedValues())
      {
        mySettingsStore.RemoveIndexedValue(PostfixTemplatesSettingsAccessor.DisabledProviders, provider.Key);
      }

      Templates.Clear();
      FillTemplates(); // todo: should not affect current page
    }
  }
}