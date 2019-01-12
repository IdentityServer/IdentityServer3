/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

/// <reference path="../libs/angular/angular.1.3.15.js" />

window.identityServer = (function () {
    "use strict";

    var identityServer = {
        getModel: function () {
            var encodedJson = document.getElementById("modelJson").textContent;
            var json = Encoder.htmlDecode(encodedJson);
            var model = JSON.parse(json);
            return model;
        }
    };

    return identityServer;
})();

(function () {
    "use strict";

    (function () {
        var app = angular.module("app", []);

        app.controller("LayoutCtrl", function ($scope, Model) {
            $scope.model = Model;
        });

        app.directive("antiForgeryToken", function () {
            return {
                restrict: 'E',
                replace: true,
                scope: {
                    token: "="
                },
                template: "<input type='hidden' name='{{token.name}}' value='{{token.value}}'>"
            };
        });

        app.directive("focusIf", function ($timeout) {
            return {
                restrict: 'A',
                scope: {
                    focusIf:'='
                },
                link: function (scope, elem, attrs) {
                    if (scope.focusIf) {
                        $timeout(function () {
                            elem.focus();
                        }, 100);
                    }
                }
            };
        });

        app.directive('iframeOnload', function () {
            var model = identityServer.getModel();
            return {
                link: function (scope, elem, attrs) {
                    elem.on('load', function (event) {
                        if (!model.autoRedirect || !model.redirectUrl) return;
                        if ($("iframe.signout").length > 1) {
                            event.target.remove();
                        } else {
                            window.location = model.redirectUrl;
                        };
                    });
                }
            };
        });
    })();

    (function () {
        var model = identityServer.getModel();
        angular.module("app").constant("Model", model);
        if (model.autoRedirect && model.redirectUrl) {
            if (model.autoRedirectDelay > 0) {
                window.setTimeout(function () {
                    window.location = model.redirectUrl;
                }, model.autoRedirectDelay * 1000);
            }
        }
    })();

})();
