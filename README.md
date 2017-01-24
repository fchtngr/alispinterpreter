A minimal Lisp Interpreter in C#
------------------------------------------

A minimal lisp interpreter written in C# for a university course I did a few years ago.

Supported Operators:

* quote, if, lambda
* Arithmetic operators: +, *, -, /
* List operations: first, rest, cons, list
* Comparisons: >, >=, <, <=, =

[For more info look here](http://dotnet.jku.at/applications/course11/Feichtinger/)

##Sample I/O

	ttl>(define x 2)
	2
	ttl>(* x 5)
	10
	ttl>(set x (+ x x))
	4
	ttl>(define square(lambda (x) (* x x)))
	lambda
	ttl>(square x)
	16
	ttl>(define compose (lambda (f g) (lambda (x) (f (g x)))))
	lambda
	ttl>((compose square square) 2)
	16
	ttl>(define  fact (lambda (n) (if (<= n 1) 1 (* n (fact (- n 1))))))
	lambda
	ttl>(fact 10)
	3628800
	ttl>

##Screenshots of the GUI

![GUI1](/img/screen1.PNG)

![GUI2](/img/screen2.PNG)
