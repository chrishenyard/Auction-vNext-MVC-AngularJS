describe('Auction timer', function () {
	var scope;
	var element;
	var rootScope;

	beforeEach(module('auctionApp'));

	beforeEach(inject(function () {
		var date = new Date();
		element = angular.element('<auction-timer closed="{{item.Closed}}" item="item"></auction-timer>');
		angular.bootstrap(element, ['auctionApp']);

		scope = element.isolateScope();
		rootScope = scope.$root;

		scope.item = {
			Id: '',
			Description: '',
			ImageUrl: '',
			HighBid: 0,
			Increment: 0,
			Processing: false,
			TimedOut: false,
			Closed: new Date(date.getUTCFullYear(), date.getUTCMonth(), date.getUTCDate() + 1)
		};
	}));

	it('shows time remaining', function () {
		rootScope.$broadcast(auction.EventTypes.TimerTick);
		var text = element[0].innerText;
		expect(text.length).toBeGreaterThan(0);
	});
});