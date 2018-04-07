function scrollJumper() {
	"use strict";

	function ScrollJumperController() {
		var ctrl = this;
		ctrl.scrolledUp = false;
		ctrl.scrolledDown = false;
		ctrl.savedPosition = 0;

		ctrl.$onInit = function () {
		}

		ctrl.isScrollable = function () {
			return (document.body.clientHeight - document.documentElement.clientHeight > 0);
		}

		ctrl.scrollDown = function () {
			if (ctrl.scrolledDown) {
				$('html, body').animate({ scrollTop: ctrl.savedPosition }, 800);
				ctrl.scrolledDown = false;
				ctrl.scrolledUp = false;
			} else {
				ctrl.savedPosition = window.pageYOffset || document.documentElement.scrollTop;
				$('html, body').animate({ scrollTop: $('body').height() }, 800);
				ctrl.scrolledDown = true;
			}
		}

		ctrl.scrollUp = function () {
			if (ctrl.scrolledUp) {
				$('html, body').animate({ scrollTop: ctrl.savedPosition }, 800);
				ctrl.scrolledDown = false;
				ctrl.scrolledUp = false;
			} else {
				ctrl.savedPosition = window.pageYOffset || document.documentElement.scrollTop;
				$('html, body').animate({ scrollTop: '0px' }, 800);
				ctrl.scrolledUp = true;
			}
		}

		ctrl.mouseover = function ($event) {
			var timeDelay = 10;
			var alphabet = ["f", "e", "c", "a", "8", "6", "4", "2", "0"];
			var color;
			var i = 0;
			var DescriptorInterval = setInterval(function () {
				color = "#" + "f" + alphabet[i] + "f" + alphabet[i] + "f" + alphabet[i];
				$event.target.style.backgroundColor = color;
				i++;
				if (i === alphabet.length) clearInterval(DescriptorInterval);
			}, timeDelay);
		}

		ctrl.mouseout = function ($event) {
			$event.target.style.backgroundColor = "#fff";
		}
	}

	angular.module("libiada", []).component("scrollJumper", {
		templateUrl: window.location.origin + "/Partial/_ScrollJumper",
		controller: [ScrollJumperController]
	});
}

scrollJumper();
