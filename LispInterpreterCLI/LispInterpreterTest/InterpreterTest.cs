using System;
using LispInterpreter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LispInterpreterTest
{
    /// <summary>
    ///This is a test class for InterpreterTest and is intended
    ///to contain all InterpreterTest Unit Tests
    ///</summary>
    [TestClass()]
    public class InterpreterTest
    {
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        [TestMethod()]
        public void TestQuote()
        {
            Interpreter target = new InterpreterImpl(); 
            string input = "(quote (+ 1 2 (+ 1 2 3)))"; 
            string expected = "(+ 1 2 (+ 1 2 3))"; 
            string actual = target.Interpret(input);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void TestList()
        {
            Interpreter target = new InterpreterImpl();
            string actual;
            actual = target.Interpret("(define x (list 1 2 3 4))");
            Assert.AreEqual("(1 2 3 4)", actual);
            actual = target.Interpret("(first x)");
            Assert.AreEqual("1", actual);
            actual = target.Interpret("(rest x)");
            Assert.AreEqual("(2 3 4)", actual);
        }

        [TestMethod()]
        public void TestSet()
        {
            Interpreter target = new InterpreterImpl();

            try
            {
                target.Interpret("(set x 3)");
                Assert.Fail();
            }
            catch (LispException e)
            {
                Assert.AreEqual(LispErrorType.SYMBOL_NOT_FOUND, e.ErrorType);
            }


            target.Interpret("(define x 3)");
            string actual = target.Interpret("(+ x x)");
            string expected = "6";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void TestCons()
        {
            Interpreter target = new InterpreterImpl();
            string actual = target.Interpret("(cons 1 2 3 (list 4 5 6))");
            
            Assert.AreEqual("(1 2 3 4 5 6)", actual);
        }

        [TestMethod()]
        public void TestArithmetic()
        {
            Interpreter target = new InterpreterImpl();
            target.Interpret("(define x 5)");
            target.Interpret("(define y 10)");

            string actual;
            actual = target.Interpret("(+ x y)");
            Assert.AreEqual("15", actual);
            actual = target.Interpret("(- y x)");
            Assert.AreEqual("5", actual);

            actual = target.Interpret("(* x x x)");
            Assert.AreEqual("125", actual);
            actual = target.Interpret("(/ y x 5)");
            Assert.AreEqual("0,4", actual);

            actual = target.Interpret("(* (* x x) y)");
            Assert.AreEqual("250", actual);

            actual = target.Interpret("(* 0,1 0,1)");
            Assert.AreEqual("0,01", actual);
        }

        [TestMethod()]
        public void TestCompare()
        {
            Interpreter target = new InterpreterImpl();
            target.Interpret("(define x 100)");
            target.Interpret("(define y 100)");
            target.Interpret("(set x 5)");
            target.Interpret("(set y 10)");

            string actual;
            actual = target.Interpret("(if (< x y) 1 -1)");
            Assert.AreEqual("1", actual);
            actual = target.Interpret("(if (> x y) 1 -1)");
            Assert.AreEqual("-1", actual);

            actual = target.Interpret("(if (<= x y) 1 -1)");
            Assert.AreEqual("1", actual);
            actual = target.Interpret("(if (>= x y) 1 -1)");
            Assert.AreEqual("-1", actual);

            actual = target.Interpret("(if (<= 1 1) 1 -1)");
            Assert.AreEqual("1", actual);
            actual = target.Interpret("(if (>= 1 1) 1 -1)");
            Assert.AreEqual("1", actual);
            actual = target.Interpret("(if (= 1 1) 1 -1)");
            Assert.AreEqual("1", actual);
        }

        [TestMethod()]
        public void TestLambda()
        {
            Interpreter target = new InterpreterImpl();
            string actual;
            actual = target.Interpret("(define pow(lambda (x) (* x x)))"); //pow(x) = x²
            Assert.AreEqual("lambda", actual);
            actual = target.Interpret("(define chain(lambda(f g) (lambda (x) (f (g x)))))"); //(chain(f,g))(x) = f(g(x))
            Assert.AreEqual("lambda", actual);
            actual = target.Interpret("((chain pow pow) 2)"); //(2²)² = 2^4 = 16
            Assert.AreEqual("16", actual);
        }

        [TestMethod()]
        public void TestFact()
        {
            Interpreter target = new InterpreterImpl();
            string actual;
            actual = target.Interpret("(define fact (lambda (n) (if (<= n 1) 1 (* n (fact (- n 1))))))");
            Assert.AreEqual("lambda", actual);
            actual = target.Interpret("(fact 10)");
            Assert.AreEqual("3628800", actual);
            actual = target.Interpret("(fact 20)");
            Assert.AreEqual("2432902008176640000", actual);
            actual = target.Interpret("(fact 25)");
            Assert.AreEqual("15511210043330985984000000", actual);
        }

        public void TestChain()
        {
            Interpreter target = new InterpreterImpl();
            string actual;
            actual = target.Interpret("(define pow(lambda (x) (* x x)))");
            Assert.AreEqual("lambda", actual);
            actual = target.Interpret("(define compose (lambda (f g) (lambda (x) (f (g x)))))");
            Assert.AreEqual("lambda", actual);
            actual = target.Interpret("((compose pow pow) 2)");
            Assert.AreEqual("16", actual);
            actual = target.Interpret("(define quad (lambda (x) ((compose pow pow) x)))");
            Assert.AreEqual("lambda", actual);
            actual = target.Interpret("(quad 2)");
            Assert.AreEqual("16", actual);
        }

        public void TestExceptions()
        {
            Interpreter target = new InterpreterImpl();

            try
            {
                target.Interpret("asdsad");
                Assert.Fail();
            }
            catch (LispException e)
            {
                Assert.AreEqual(LispErrorType.PARENTHESIS_MISMATCH, e.ErrorType);
            }

            try
            {
                target.Interpret("(asdsad asd)");
                Assert.Fail();
            }
            catch (LispException e)
            {
                Assert.AreEqual(LispErrorType.SYMBOL_NOT_FOUND, e.ErrorType);
            }

            try
            {
                target.Interpret("((asdsad)");
                Assert.Fail();
            }
            catch (LispException e)
            {
                Assert.AreEqual(LispErrorType.PARENTHESIS_MISMATCH, e.ErrorType);
            }

            try
            {
                target.Interpret("(+ 1 (* 1 2)))");
                Assert.Fail();
            }
            catch (LispException e)
            {
                Assert.AreEqual(LispErrorType.PARENTHESIS_MISMATCH, e.ErrorType);
            }
        }
    }
}
