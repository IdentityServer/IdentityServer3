/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

/// <reference path="../libs/angular/angular.1.2.13.js" />

(function () {
    "use strict";

    var app = angular.module("app", []);

    app.controller("LayoutCtrl", function ($scope, Model) {
        $scope.model = Model;
    });

    app.directive("antiForgeryToken", function () {
        return {
            restrict: 'E',
            replace:true,
            scope:{
                token:"="
            },
            template: "<input type='hidden' name='{{token.name}}' value='{{token.value}}'>"
        };
    });
})();

(function () {
    var json = document.getElementById("modelJson").textContent;
    var model = JSON.parse(json);
    angular.module("app").constant("Model", model);
})();
