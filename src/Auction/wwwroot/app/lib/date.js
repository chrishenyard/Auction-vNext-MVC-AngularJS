'use strict';

(function (date) {
	var msecPerSecond = 1000;
	var msecPerMinute = msecPerSecond * 60;
	var msecPerHour = msecPerMinute * 60;
	var msecPerDay = msecPerHour * 24;

	function DateDiff(endDate, now) {
		now = now || new Date();

		var interval = Math.max(endDate.getTime() - now.getTime(), 0);

		this.daysDiffNow = Math.floor(interval / msecPerDay);
		interval = interval - (this.daysDiffNow * msecPerDay);

		this.hoursDiffNow = Math.floor(interval / msecPerHour);
		interval = interval - (this.hoursDiffNow * msecPerHour);

		this.minutesDiffNow = Math.floor(interval / msecPerMinute);
		interval = interval - (this.minutesDiffNow * msecPerMinute);

		this.secondsDiffNow = Math.floor(interval / msecPerSecond);

		this.timeRemaining = this.daysDiffNow + this.hoursDiffNow + this.minutesDiffNow + this.secondsDiffNow;
	}

	DateDiff.prototype.toString = function () {
		var hoursString = ('0' + this.hoursDiffNow).slice(-2) + 'h';
		var minutesString = ('0' + this.minutesDiffNow).slice(-2) + 'm';
		var secondsString = ('0' + this.secondsDiffNow).slice(-2) + 's';

		return hoursString + ' ' + minutesString + ' ' + secondsString;
	};


	date.DateDiff = DateDiff

})(auction.date = auction.date || {});