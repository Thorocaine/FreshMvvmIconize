﻿//I copied this code from https://github.com/rid00z/FreshMvvm

using FormsPlugin.Iconize;
using FreshMvvm;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace FreshMvvmIconize
{
    public class FreshIconTabbedNavigationContainer : IconTabbedPage, IFreshNavigationService
    {
        List<Page> _tabs = new List<Page>();
        public IEnumerable<Page> TabbedPages => _tabs;

        public FreshIconTabbedNavigationContainer() : this(Constants.DefaultNavigationServiceName)
        {

        }

        public FreshIconTabbedNavigationContainer(string navigationServiceName)
        {
            NavigationServiceName = navigationServiceName;
            RegisterNavigation();
        }

        protected void RegisterNavigation() => FreshIOC.Container.Register<IFreshNavigationService>(this, NavigationServiceName);

        public virtual Page AddTab<T>(string title, string icon, object data = null) where T : FreshBasePageModel
        {
            var page = FreshPageModelResolver.ResolvePageModel<T>(data);
            page.GetModel().CurrentNavigationServiceName = NavigationServiceName;
            _tabs.Add(page);
            var navigationContainer = CreateContainerPageSafe(page);
            navigationContainer.Title = title;
            if (!string.IsNullOrWhiteSpace(icon))
                navigationContainer.Icon = icon;
            Children.Add(navigationContainer);
            return navigationContainer;
        }

        internal Page CreateContainerPageSafe(Page page)
        {
            if (page is NavigationPage || page is MasterDetailPage || page is TabbedPage)
                return page;

            return CreateContainerPage(page);
        }

        protected virtual Page CreateContainerPage(Page page) => new FreshIconNavigationContainer(page);

        public Task PushPage(Page page, FreshBasePageModel model, bool modal = false, bool animate = true) => (modal)
                ? CurrentPage.Navigation.PushModalAsync(CreateContainerPageSafe(page))
                : CurrentPage.Navigation.PushAsync(page);

        public Task PopPage(bool modal = false, bool animate = true) => (modal)
                ? CurrentPage.Navigation.PopModalAsync(animate)
                : CurrentPage.Navigation.PopAsync(animate);

        public Task PopToRoot(bool animate = true) => CurrentPage.Navigation.PopToRootAsync(animate);

        public string NavigationServiceName { get; private set; }

        public void NotifyChildrenPageWasPopped()
        {
            foreach (var page in Children)
            {
                if (page is NavigationPage)
                    ((NavigationPage)page).NotifyAllChildrenPopped();
            }
        }

        public Task<FreshBasePageModel> SwitchSelectedRootPageModel<T>() where T : FreshBasePageModel
        {
            var page = _tabs.FindIndex(o => o.GetModel().GetType().FullName == typeof(T).FullName);

            if (page > -1)
            {
                CurrentPage = Children[page];
                var topOfStack = CurrentPage.Navigation.NavigationStack.LastOrDefault();
                if (topOfStack != null)
                    return Task.FromResult(topOfStack.GetModel());

            }
            return null;
        }
    }
}
