/*price range*/

 $('#sl2').slider();

	var RGBChange = function() {
	  $('#RGB').css('background', 'rgb('+r.getValue()+','+g.getValue()+','+b.getValue()+')')
	};	
		
/*scroll to top*/

$(document).ready(function(){
	$(function () {
		$.scrollUp({
	        scrollName: 'scrollUp', // Element ID
	        scrollDistance: 300, // Distance from top/bottom before showing element (px)
	        scrollFrom: 'top', // 'top' or 'bottom'
	        scrollSpeed: 300, // Speed back to top (ms)
	        easingType: 'linear', // Scroll to top easing (see http://easings.net/)
	        animation: 'fade', // Fade, slide, none
	        animationSpeed: 200, // Animation in speed (ms)
	        scrollTrigger: false, // Set a custom triggering element. Can be an HTML string or jQuery object
					//scrollTarget: false, // Set a custom target element for scrolling to the top
	        scrollText: '<i class="fa fa-angle-up"></i>', // Text for element, can contain HTML
	        scrollTitle: false, // Set a custom <a> title if required.
	        scrollImg: false, // Set true to use image
	        activeOverlay: false, // Set CSS color to display scrollUp active point, e.g '#00FFFF'
	        zIndex: 2147483647 // Z-Index for the overlay
		});
	});
});
const forms = document.querySelector(".forms"),
	pwShowHide = document.querySelectorAll(".eye-icon"),
	links = document.querySelectorAll(".link");

pwShowHide.forEach(eyeIcon => {
	eyeIcon.addEventListener("click", () => {
		let pwFields = eyeIcon.parentElement.parentElement.querySelectorAll(".password");

		pwFields.forEach(password => {
			if (password.type === "password") {
				password.type = "text";
				eyeIcon.classList.replace("bx-hide", "bx-show");
				return;
			}
			password.type = "password";
			eyeIcon.classList.replace("bx-show", "bx-hide");
		})

	})
})

links.forEach(link => {
	link.addEventListener("click", e => {
		e.preventDefault(); //preventing form submit
		forms.classList.toggle("show-signup");
	})
})
const appDispatcher = new Flux.Dispatcher();

const actionTypes = {
	SELECT_FILTER: 'SELECT_FILTER',
	DESELECT_FILTER: 'DESELECT_FILTER'
};

const actions = {
	selectFilter(filter) {
		appDispatcher.dispatch({
			type: 'SELECT_FILTER',
			text: filter.name
		});
	},
	deselectFilter(filter) { }
}

const state = {};

appDispatcher.register(function (payload) {

	switch (payload.type) {
		case actionTypes.SELECT_FILTER:
			console.log('HEYO');
			return state;

		case actionTypes.DESELECT_FILTER:
			console.log('HEYO');
			return state;
		default:
			console.log('oops');
			return state;
	}

});


var filters = {};
$('.container').on('click', 'li', function () {
	var $li = $(this)
	if ($li.hasClass('reset')) {
		$li.parent().find('li.active').each(function () {
			$(this).removeClass('active')
		})
		filters = {};
	} else {
		if ($li.hasClass('active')) {
			$li.removeClass('active')
			delete filters[$li.text().trim()]
		} else {
			$li.addClass('active')
			filters[$li.text().trim()] = true
		}
	}
	printFilters()
})

var printFilters = function () {
	var filtersList = Object.keys(filters);
	if (filtersList.length) {
		$('.filters').text(filtersList.join(', '))
	} else {
		$('.filters').text('[NONE]');
	}

}

