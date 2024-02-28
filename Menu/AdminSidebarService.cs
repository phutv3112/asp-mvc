using System.Collections.Generic;
using System.Text;
using AppMVC.Menu;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;

namespace AppMVC.Menu
{
    public class AdminSidebarService
    {
        private readonly IUrlHelper UrlHelper;
        public List<SidebarItem> Items { get; set; } = new List<SidebarItem>();


        public AdminSidebarService(IUrlHelperFactory factory, IActionContextAccessor action)
        {
            UrlHelper = factory.GetUrlHelper(action.ActionContext);
            // Khoi tao cac muc sidebar

            Items.Add(new SidebarItem() { Type = SidebarItemType.Divider });
            Items.Add(new SidebarItem() { Type = SidebarItemType.Heading, Title = "Manager" });

            Items.Add(new SidebarItem()
            {
                Type = SidebarItemType.NavItem,
                Controller = "DbManage",
                Action = "Index",
                Area = "Database",
                Title = "Manage Database",
                AwesomeIcon = "fas fa-database"
            });
            Items.Add(new SidebarItem() { Type = SidebarItemType.Divider });
            Items.Add(new SidebarItem()
            {
                Type = SidebarItemType.NavItem,
                Controller = "Contact",
                Action = "Index",
                Area = "Contact",
                Title = "Manage Contact",
                AwesomeIcon = "far fa-address-card"
            });
           
            Items.Add(new SidebarItem() { Type = SidebarItemType.Divider });

            Items.Add(new SidebarItem()
            {
                Type = SidebarItemType.NavItem,
                Title = "Manage Posts",
                AwesomeIcon = "fab fa-usps",
                CollapseId = "blog",
                Items = new List<SidebarItem>() {
                        new SidebarItem() {
                                Type = SidebarItemType.NavItem,
                                Controller = "Category",
                                Action = "Index",
                                Area = "Blog",
                                Title = "Categories",
                        },
                         new SidebarItem() {
                                Type = SidebarItemType.NavItem,
                                Controller = "Category",
                                Action = "Create",
                                Area = "Blog",
                                Title = "Create Category"
                        },
                        new SidebarItem() {
                                Type = SidebarItemType.NavItem,
                                Controller = "Post",
                                Action = "Index",
                                Area = "Blog",
                                Title = "Posts"
                        },
                        new SidebarItem() {
                                Type = SidebarItemType.NavItem,
                                Controller = "Post",
                                Action = "Create",
                                Area = "Blog",
                                Title = "Create Post"
                        },
                    },
            });
            Items.Add(new SidebarItem() { Type = SidebarItemType.Divider });
            Items.Add(new SidebarItem()
            {
                Type = SidebarItemType.NavItem,
                Title = "Manage Products",
                AwesomeIcon = "fas fa-leaf",
                CollapseId = "product",
                Items = new List<SidebarItem>() {
                        new SidebarItem() {
                                Type = SidebarItemType.NavItem,
                                Controller = "CategoryProduct",
                                Action = "Index",
                                Area = "Product",
                                Title = "Product Categories"
                        },
                         new SidebarItem() {
                                Type = SidebarItemType.NavItem,
                                Controller = "CategoryProduct",
                                Action = "Create",
                                Area = "Product",
                                Title = "Create Product Category"
                        },
                        new SidebarItem() {
                                Type = SidebarItemType.NavItem,
                                Controller = "ProductManager",
                                Action = "Index",
                                Area = "Product",
                                Title = "Products"
                        },
                        new SidebarItem() {
                                Type = SidebarItemType.NavItem,
                                Controller = "ProductManager",
                                Action = "Create",
                                Area = "Product",
                                Title = "Create Product",
                        },
                    },
            });

        }


        public string renderHtml()
        {
            var html = new StringBuilder();

            foreach (var item in Items)
            {
                html.Append(item.RenderHtml(UrlHelper));
            }


            return html.ToString();
        }

        public void SetActive(string Controller, string Action, string Area)
        {
            foreach (var item in Items)
            {
                if (item.Controller == Controller && item.Action == Action && item.Area == Area)
                {
                    item.IsActive = true;
                    return;
                }
                else
                {
                    if (item.Items != null)
                    {
                        foreach (var childItem in item.Items)
                        {
                            if (childItem.Controller == Controller && childItem.Action == Action && childItem.Area == Area)
                            {
                                childItem.IsActive = true;
                                item.IsActive = true;
                                return;

                            }
                        }
                    }
                }



            }
        }


    }
}