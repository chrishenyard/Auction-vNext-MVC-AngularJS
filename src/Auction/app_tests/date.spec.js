describe('DateDiff', function () {
	it('time remaining', function () {
		var now = new Date('2015/1/31');
		var endDate = new Date(2015, 0, 31, 3, 30, 30, 0);

		var dateDiff = new auction.date.DateDiff(endDate, now);

		expect(dateDiff.daysDiffNow).toEqual(0);
		expect(dateDiff.hoursDiffNow).toEqual(3);
		expect(dateDiff.minutesDiffNow).toEqual(30);
		expect(dateDiff.secondsDiffNow).toEqual(30);
	});
});