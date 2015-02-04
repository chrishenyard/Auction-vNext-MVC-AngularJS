'use strict';

window.auction = window.auction || {};

auction.EventTypes = {
	TimerTimeout: 'timer-timeout',
	TimerTick: 'timer-tick',
	SlideShowToggle: 'slide-show-toggle'
};

(function (models) {
	function AuctionBid() {
		this.UserId = '';
		this.AuctionItemId = '';
		this.Bid = 0;
		this.HighBid = false;
		this.Timestamp = new Date();
	}

	models.AuctionBid = AuctionBid;

})(auction.models = auction.models || {});

(function (processors) {
	function Bid($q, auctionDataFactory, item, user) {
		this.$q = $q;
		this.item = item;
		this.auctionDataFactory = auctionDataFactory;
		this.user = user;
	}

	Bid.prototype.saveBid = function () {
		var deferred = this.$q.defer();
		var bid = new auction.models.AuctionBid();
		bid.UserId = this.user.Id;
		bid.AuctionItemId = this.item.Id;
		bid.Bid = this.item.HighBid;

		this.auctionDataFactory.saveBid(bid)
			.success(function (data, status, headers, config) {
				deferred.resolve(data);
			})
			.error(function (data, status, headers, config) {
				var message = 'Error posting bid. Status: ' + status + ' Method: ' + config.method + '. Url: ' + config.url;
				deferred.reject(message);
			});

		return deferred.promise;
	}

	Bid.prototype.getAuctionBids = function () {
		var deferred = this.$q.defer();

		this.auctionDataFactory.getAuctionBids(this.item.Id)
			.success(function (data, status, headers, config) {
				deferred.resolve(data);
			})
			.error(function (data, status, headers, config) {
				var message = 'Error getting bids. Status: ' + status + ' Method: ' + config.method + '. Url: ' + config.url;
				deferred.reject(message);
			});

		return deferred.promise;
	};

	processors.Bid = Bid;

})(auction.processors = auction.processors || {});

(function () {
	var ctrl = function ($scope, $q, auctionDataFactory, timerService) {
		var ctrl = this;
		ctrl.status = '';
		ctrl.error = false;
		ctrl.initialized = false;
		ctrl.items = [];
		ctrl.users = [];
		ctrl.winner = { Id: '', FirstName: 'No', LastName: 'Winner' };
		ctrl.autoBidDefinition = {
			running: false,
			handle: -1,
			interval: 3
		};

		init(auctionDataFactory, $q);

		function init(auctionDataFactory, $q) {
			console.info('Initializing');
			var itemsDeferred = $q.defer();
			var usersDeferred = $q.defer();

			auctionDataFactory.getItems()
				.success(function (data, status, headers, config) {
					ctrl.items = data;
					itemsDeferred.resolve();
				})
				.error(function (data, status, headers, config) {
					ctrl.status = 'Unable to load audit data. Status: ' + status + ' Method: ' + config.method + '. Url: ' + config.url;
					console.error(data);
					itemsDeferred.reject();
				});

			auctionDataFactory.getUsers()
				.success(function (data, status, headers, config) {
					ctrl.users = data;
					usersDeferred.resolve();
				})
				.error(function (data, status, headers, config) {
					ctrl.status = 'Unable to load user data. Status: ' + status + ' Method: ' + config.method + '. Url: ' + config.url;
					console.error(data);
					usersDeferred.reject();
				});

			$q.all([itemsDeferred.promise, usersDeferred.promise]).then(
				function () {
					timerService.startTimer($scope);
				},
				function () {
					ctrl.error = true;
			}).finally(
				function () {
					console.info('Done initializing');
					ctrl.initialized = true;
				}
			);
		}

		ctrl.timedOut = function (event, item) {
			console.info('Timed out');
			item.TimedOut = true;
			var bid = new auction.processors.Bid($q, auctionDataFactory, item);
			$scope.$broadcast(auction.EventTypes.SlideShowToggle, { item: item, id: 'processing' });

			bid.getAuctionBids().then(
				function (data) {
					console.info(data);
					item.winner = { Id: '', FirstName: 'No', LastName: 'Winner' };

					if (data[0]) {
						var winner = $.grep(ctrl.users, function (element) {
							return (element.Id === data[0].UserId);
						});

						item.winner = { Id: winner[0].Id, FirstName: winner[0].FirstName, LastName: winner[0].LastName };
					}

					setTimeout(function () {
						$scope.$broadcast(auction.EventTypes.SlideShowToggle, { item: item, id: 'winner' });
					}, 2000);
				},
				function (message) {
					console.error(message);
				}).finally(
				function () {
					console.info('Done timing out');
				});
		};

		$scope.$on(auction.EventTypes.TimerTimeout, ctrl.timedOut);

		ctrl.saveBid = function (item) {
			console.info('Saving bid');
			item.Processing = true;
			var index = Math.floor((Math.random() * ctrl.users.length) + 0);
			var user = ctrl.users[index];
			var bid = new auction.processors.Bid($q, auctionDataFactory, item, user);

			bid.saveBid().then(
				function (data) {
					console.info(data);
					item.HighBid = data.Bid;
					item.Outbid = !data.HighBid;
				},
				function (message) {
					console.error(message);
			}).finally(
				function () {
					setTimeout(function () {
						console.info('Done saving bid');
						item.Processing = false;
					}, 500);
				});
		};

		ctrl.autoBid = function () {
			if (!ctrl.autoBidDefinition.running) {
				ctrl.autoBidDefinition.running = true;

				ctrl.autoBidDefinition.handle = setInterval(function () {
					var index = Math.floor((Math.random() * ctrl.items.length) + 0);
					var item = ctrl.items[index];
					ctrl.saveBid(item);
				}, ctrl.autoBidDefinition.interval * 1000);
			} else {
				ctrl.autoBidDefinition.running = false;
				clearInterval(ctrl.autoBidDefinition.handle);
			}
		};
	};

	angular.module('auctionApp', ['ngAnimate', 'auctionServices'])
		.controller('auctionCtrl', ['$scope', '$q', 'auctionDataFactory', 'timerService', ctrl]);
})();
