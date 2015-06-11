﻿/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

/// <reference path="../libs/angular/angular.1.3.15.js" />

(function () {
    "use strict";

    (function () {
        var app = angular.module("app", []);

        app.controller("LayoutCtrl",
            ['$scope', 'Model',
                function ($scope, Model) {
                    $scope.model = Model;
                }]);

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
    })();

    (function () {
        var modelJson = document.getElementById("modelJson");
        var encodedJson = '';
        if (typeof (modelJson.textContent) !== undefined) {
            encodedJson = modelJson.textContent;
        } else {
            encodedJson = modelJson.innerHTML;
        }
        var json = Encoder.htmlDecode(encodedJson);
        var model = JSON.parse(json);
        angular.module("app").constant("Model", model);
        if (model.autoRedirect && model.redirectUrl) {
            if (model.autoRedirectDelay < 0) {
                model.autoRedirectDelay = 0;
            }
            window.setTimeout(function () {
                window.location = model.redirectUrl;
            }, model.autoRedirectDelay * 1000);
        }
    })();

})();
