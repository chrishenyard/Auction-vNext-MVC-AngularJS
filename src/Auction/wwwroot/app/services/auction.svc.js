'use strict';

(function () {
	var auctionDataFactory = function ($http) {
		var urlBase = 'http://localhost:5000';
		var dataFactory = {};

		dataFactory.getItems = function () {
			var url = urlBase + '/Auction/GetItems';
			return $http.get(url);
		};

		dataFactory.saveBid = function (bid) {
			var url = urlBase + '/Auction/SaveBid';
			return $http.post(url, bid);
		};

		dataFactory.getUsers = function () {
			var url = urlBase + '/Auction/GetUsers';
			return $http.get(url);
		}

		dataFactory.getConfig = function () {
			var url = urlBase + '/Config';
			return $http.get(url);
		};

		dataFactory.getAuctionBids = function (id) {
			var url = urlBase + '/Auction/GetAuctionBids/' + id;
			return $http.get(url);
		};

		return dataFactory;
	};

	var timerService = function ($interval) {
		this.timers = [];

		this.startTimer = function (scope, event, interval) {
			event = event || auction.EventTypes.TimerTick;
			interval = interval || 1000;

			if (this.timers[event]) return;

			this.timers[event] = $interval(function () {
				scope.$broadcast(event);
			}, interval);
		};

		this.stopTimer = function (event) {
			$interval.cancel(this.timers[event]);
		};
	};

	var auctionTimer = function () {
		function link(scope, element, attrs) {
			var listener = scope.$on(auction.EventTypes.TimerTick, onTimerTick);
			var _scope = scope;

			function onTimerTick() {
				var d = new auction.date.DateDiff(new Date(_scope.item.Closed));
				element.text(d.toString());

				if (d.timeRemaining === 0) {
					listener();
					_scope.$emit(auction.EventTypes.TimerTimeout, _scope.item);
				}
			}
		}

		return {
			restrict: 'E',
			link: link,
			scope: { item: '='}
		};
	};

	var auctionButtonSpinner = function () {
		function link(scope, element, attr) {
			var ladda = Ladda.create(element[0]);
			var disable = typeof attr.auctionButtonSpinnerDisable === 'undefined' ? false : (attr.auctionButtonSpinnerDisable.toLowerCase() === 'true');

			scope.$watch(attr.auctionButtonSpinner, function (newVal, oldVal) {
				if (angular.isNumber(oldVal)) {
					if (angular.isNumber(newVal)) {
						ladda.setProgress(newVal);
					} else {
						newVal && ladda.setProgress(0) || ladda.stop();
					}
				} else {
					newVal && ladda.start(disable) || ladda.stop();
				}
			});
		}

		return {
			restrict: 'A',
			link: link
		}
	};

	var auctionHighlight = function () {
		function link(scope, element, attr) {
			var highlightClass = 'currency-amount-highlight';

			scope.$watch(attr.auctionHighlight, function (newVal, oldVal) {
				if (newVal != oldVal) {
					element.clearQueue().queue(function (next) {
						$(this).addClass(highlightClass); next();
					}).delay(1000).queue(function (next) {
						$(this).removeClass(highlightClass); next();
					});
				}
			});
		}

		return {
			restrict: 'A',
			link: link,
			scope: { item: '=' }
		}
	};

	var auctionElementSpinner = function () {
		function link(scope, element, attr) {
			var spinner = createSpinner(element[0]);
			var spinElement = spinner.spin().el;
			$(spinElement).css('position', 'relative');
			element[0].appendChild(spinElement);
		}
				
		function createSpinner( element ) {

			var height = element.offsetHeight,
				spinnerColor;

			if( height === 0 ) {
				height = parseFloat( window.getComputedStyle( element ).height );
			}

			if( height > 32 ) height *= 0.8;

			var lines = 12,
				radius = height * 0.2,
				length = radius * 0.6,
				width = radius < 7 ? 2 : 3;

			return new Spinner( {
				color: '#000',
				lines: lines,
				radius: radius,
				length: length,
				width: width,
				zIndex: 'auto',
				top: '50%',
				left: '50%',
				className: ''
			} );

		}
				
		return {
			link: link
		};
	};

	var auctionSlideShow = function () {
		function link($scope, $element, $attrs) {
			var slideShowIds = [];
			var selectedSlideShowId = '';
			var itemId = $attrs.itemId;
			init();

			function init() {
				var children = $element.children();
				var showId = $($element.children()[0]).attr('id');

				children.each(function (index) {
					slideShowIds[$(this).attr('id')] = false;
				});

				slideShowIds[showId] = true;
				selectedSlideShowId = showId;
			}

			$scope.show = function (id) {
				return slideShowIds[id];
			};

			function handleEvent(item) {
				return item.Id === itemId;
			}

			var slideShowToggle = function (event, data) {
				if (!handleEvent(data.item)) return;
				slideShowIds[selectedSlideShowId] = false;
				slideShowIds[data.id] = true;
				selectedSlideShowId = data.id;
			};

			$scope.$on('slide-show-toggle', slideShowToggle);
		}

		return {
			link: link,
			scope: true
		};
	};

	angular.module('auctionServices', [])
		.factory('auctionDataFactory', ['$http', auctionDataFactory])
		.service('timerService', ['$interval', timerService])
		.directive('auctionTimer', [auctionTimer])
		.directive('auctionButtonSpinner', [auctionButtonSpinner])
		.directive('auctionHighlight', [auctionHighlight])
		.directive('auctionElementSpinner', [auctionElementSpinner])
		.directive('auctionSlideShow', [auctionSlideShow]);
})();
