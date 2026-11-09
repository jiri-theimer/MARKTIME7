var _last_clicker;

(function ($, window) {
    var menus = {};

    $.fn.contextMenu = function (settings) {
        var $menu = $(settings.menuSelector);
        var clicker = settings.menuClicker;
        var menuEntity = settings.menuEntity;
        var menuPid = settings.menuPid;
        var menuSource = settings.menuSource;
        var menuMasterEntity = settings.menuMasterEntity;
        var menuLoadByServer = settings.menuLoadByServer;
        var menuWithoutCache = settings.menuWithoutCache;
        
        $menu.data("menuSelector", settings.menuSelector);
        if ($menu.length === 0) return;
        
        menus[settings.menuSelector] = { $menu: $menu, settings: settings };

        //make sure menu closes on any click
        $(document).click(function (e) {
            if (e.target.id === "divZoomHeader") return;
            hideAll();
        });
        $(document).on("click", function (e) {

            if (e.target.id === "divZoomHeader") return;

            var $ul = $(e.target).closest("ul");
            if ($ul.length === 0 || !$ul.data("menuSelector")) {
                hideAll();


                var b = false;
                if (e.target === clicker) b = true;
                if (b === false && (e.target.parentNode)) {
                    if (e.target.parentNode === clicker) b = true;
                }

                if (b === true) {        //rovnou při úvodní incializaci vyvolat click a zobrazit menu                       
                    _last_clicker = clicker;
                    return handler_show_menu(e, settings.menuSelector);

                }
            }
        });
        $(document).on("keydown", function (e) {
            if (e.keyCode === 27) {
                hideAll();  //ESCAPE klávesa

            }

        });

        

        // Open context menu
        (function (element, menuSelector) {
            element.on("click", function (e) {
                if (menuWithoutCache === true)
                {
                    return; //tady vypadnout, protože není požadováno zacachovat obsah okna!!!
                }
                

                // return native menu if pressing control
                if (clicker === _last_clicker) {
                    var m = getMenu(menuSelector);
                    if (m.$menu.css("display") === "block") {
                        hideAll();
                        return false;
                    }
                }
                _last_clicker = clicker;

                return handler_show_menu(e, menuSelector);

            });
        })($(this), settings.menuSelector);

        function handler_show_menu(e, menuSelector) {
            if (e.ctrlKey) return;
            hideAll();
            var menu = getMenu(menuSelector);
            if (menuLoadByServer === false) {
                //statické menu
                showMenu(menu, e);
                callOnMenuShow(menu);
                return false;
            }            
            
            
            var period="";
            if (typeof _period_get_as_string==="function")
            {
               period = _period_get_as_string();    //převzít nastavení časového období
            }
            
            $.post("/Menu/ContextMenu", { entity: menuEntity, pid: menuPid, source: menuSource, master_entity: menuMasterEntity, period: period, device: _device.type }, function (data) {
                //menu položky natahované dynamicky ze serveru
                
                $(menuSelector).html(data);

                

                //až nyní je menu stažené ze serveru
                showMenu(menu, e);



                $(".cm_submenu").each(function () { //vnořené submenu
                    var scroll = $(window)["scrollTop"]();
                    var posy = $(this).offset().top;
                    var rect = this.getBoundingClientRect();

                    if (_device.type === "Desktop" && (posy - scroll + $(this).height() > $(window).height())) {
                        
                                                
                        var mty = -1 * $(this).height()+20;
                        
                        $(this).css("margin-top",mty);
                        
                    }

                    
                });


                callOnMenuShow(menu);
                return false;
            });



        }

        //zobrazení menu: desktop u kurzoru (beze změny), telefon jako panel zdola (třída cm_mobile, contextmenu.css)
        function showMenu(menu, e) {
            const $m = menu.$menu;
            const phone = _device.isMobile;
            if (phone && $m.hasClass("cm_left2right")) $m.removeClass("cm_left2right").addClass("cm_mobile");     //okno se mohlo od vytvoření menu zúžit/rozšířit
            if (!phone && $m.hasClass("cm_mobile")) $m.removeClass("cm_mobile").addClass("cm_left2right");

            $m.data("invokedOn", $(e.target)).show();
            if (!$m.hasClass("cm_mobile")) {
                $m.css({
                    position: "absolute",
                    left: getMenuPosition(e.clientX, "width", "scrollLeft"),
                    top: getMenuPosition(e.clientY, "height", "scrollTop")
                }).off("click");
                return;
            }

            $m.css({ position: "", left: "", top: "" }).off("click").scrollTop(0);
            $m.find("li.cm-open").removeClass("cm-open");     //vždy začít se sbalenými podmenu
            if ($m.children(".cm-sheet-head").length === 0) {
                $m.prepend("<div class='cm-sheet-head'><span class='cm-sheet-grip'></span><button type='button' class='btn-close' aria-label='Zavřít'></button></div>");
            }
            //podmenu jako akordeon (na dotyku není hover): šipka u položky, položka bez odkazu rozbalí klepnutím
            $m.find("li").has("> ul.cm_submenu").each(function () {
                const $li = $(this);
                if ($li.hasClass("cm-has-sub")) return;
                $li.addClass("cm-has-sub");
                if ($li.children("a[href]").length === 0) $li.addClass("cm-sub-nolink");
                $li.children("ul.cm_submenu").before("<button type='button' class='cm-sub-toggle' aria-label='Rozbalit'><span class='material-icons-outlined-btn'>expand_more</span></button>");
            });
            $m.on("click", "li.cm-has-sub", function (ev) {
                const $li = $(this);
                if ($(ev.target).closest("ul.cm_submenu").closest("li.cm-has-sub").is($li)) return;     //položka podmenu – normální klik
                if (!$(ev.target).closest(".cm-sub-toggle").length && !$li.hasClass("cm-sub-nolink")) return;  //odkaz nadřízené položky – normální klik
                ev.preventDefault();
                ev.stopPropagation();       //nezavírat menu (document click → hideAll)
                $li.toggleClass("cm-open");
            });
            if (!document.getElementById("cm_backdrop")) $("body").append("<div id='cm_backdrop'></div>");
            $("#cm_backdrop").show();
        }

        function getMenu(menuSelector) {
            var menu = null;
            $.each(menus, function (i_menuSelector, i_menu) {
                if (i_menuSelector === menuSelector) {
                    menu = i_menu
                    return false;
                }
            });
            return menu;
        }
        function hideAll() {
            $.each(menus, function (menuSelector, menu) {
                menu.$menu.hide();
                callOnMenuHide(menu);
            });
            $("#cm_backdrop").hide();
            




        }

        function callOnMenuShow(menu) {
            var $invokedOn = menu.$menu.data("invokedOn");
            if ($invokedOn && menu.settings.onMenuShow) {
                menu.settings.onMenuShow.call(this, $invokedOn);
            }
        }
        function callOnMenuHide(menu) {
            var $invokedOn = menu.$menu.data("invokedOn");
            menu.$menu.data("invokedOn", null);
            if ($invokedOn && menu.settings.onMenuHide) {
                menu.settings.onMenuHide.call(this, $invokedOn);
            }
        }

        function getMenuPosition(mouse, direction, scrollDir) {
            var win = $(window)[direction](),
                scroll = $(window)[scrollDir](),
                menu = $(settings.menuSelector)[direction](),   //výška/šířka menu
                position = mouse + scroll;                      //souřadnice

            
            if (direction === "width") {
                
                if ($(window).width() - position<350)
                {
                    position = $(window).width()-350;
                }
                position = position + 20;
                
                if (_device.type === "Phone") {
                    position = 0;
                }
                
            }
            
            if (direction === "height" && menu + position > $(window).height()) {
                //position = scroll + $(window).height() - menu - 10;
                
                if (mouse + menu > $(window).height()) {
                    position = scroll + ($(window).height() - menu);
                } else {
                    position = scroll + mouse;
                }
                
                if (_device.type === "Phone") {
                    
                    position = scroll;
                }

                return position;

            }

            if (menu === 0 || menu === win) {
                menu = 300;
            }
            // opening menu would pass the side of the page
            if (mouse + menu > win && menu < mouse) {
                position -= menu;
            }



            return position;
        }
        return this;
    };
})(jQuery, window);