$(document).ready(function(){
	$(function () {
  		$('[data-toggle="tooltip"]').tooltip()
	});
	$('#redo').mouseenter(function(){
		$('.fa-repeat').css({'transition':'all .7s', 'transform':'rotate(359deg)'});
	});
	$('#redo').mouseleave(function(){
		$('.fa-repeat').css({'transition':'all 0s', 'transform':'rotate(1deg)'});
	});
	$('#stop').mouseenter(function(){
		$('.fa-stop').css({'transition':'all .2s', 'transform':'scale(1.2,1.2)'});
	});
	$('#stop').mouseleave(function(){
		$('.fa-stop').css('transform','scale(1.0,1.0)');
	});
	$('#review').mouseenter(function(){
		$('.fa-pencil-square-o').css({'transition':'all .2s', 'transform':'scale(1.2,1.2)'});
	});
	$('#review').mouseleave(function(){
		$('.fa-pencil-square-o').css('transform','scale(1.0,1.0)');
	});
	$('#start').mouseenter(function(){
		$('.fa-circle').css({'transition':'all .2s', 'transform':'scale(1.2,1.2)'});
	});
	$('#start').mouseleave(function(){
		$('.fa-circle').css('transform','scale(1.0,1.0)');
	});
	$('#play').mouseenter(function(){
		$('.fa-volume-up').css({'transition':'all .2s', 'transform':'scale(1.2,1.2)'});
	});
	$('#play').mouseleave(function(){
		$('.fa-volume-up').css('transform','scale(1.0,1.0)');
	});
	$('#submit1').mouseenter(function(){
		$('.fa-upload').css({'transition':'all .2s', 'transform':'scale(1.2,1.2)'});
	});
	$('#submit1').mouseleave(function(){
		$('.fa-upload').css('transform','scale(1.0,1.0)');
	});
	$('.questionclicked').click(function(){
		$('.questionshown').css('opacity','1');
		$('.alert1').css('opacity','1');
		$('.alert2').css('opacity','1');
	});
	$('.questionclicked').click(function(){
		$('.questiondefault').css('display','none');
	});
});
