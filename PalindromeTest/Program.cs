﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PalindromeTest
{
    internal class Program
    {
        private static bool IsPrime(int number)
        {
            var upperBound = (int) Math.Sqrt(number + 1);
            for (var i = 2; i <= upperBound; ++i)
            {
                if (number % i == 0)
                    return false;
            }

            return true;
        }

        private static int[] FindPrimes(int start, int end)
        {
            var count = end - start;
            return Enumerable.Range(start, count).AsParallel().Where(IsPrime).ToArray();
        }

        private static bool IsPalindrome(long number)
        {
            var digits = new List<long>();
            while (number > 0)
            {
                digits.Add(number % 10);
                number /= 10;
            }
            return IsPalindrome(digits);
        }

        private static bool IsPalindrome(IReadOnlyList<long> digits)
        {
            var toCheck = digits.Count / 2;
            for (var i = 0; i <= toCheck; ++i)
            {
                if (digits[i] != digits[digits.Count - i - 1])
                    return false;
            }

            return true;
        }

        private static (long max, long first, long second) FindLargestPalindrome(int[] primes)
        {
            var max = 0L;
            long first = 0;
            long second = 0;

            Parallel.ForEach(primes, firstPrime =>
            {
                foreach (var secondPrime in primes)
                {
                    var product = (long)firstPrime * secondPrime;
                    if (!IsPalindrome(product))
                        continue;

                    //Console.WriteLine($"{product} = {firstPrime} * {secondPrime}");
                    if (product <= max)
                        continue;

                    Interlocked.Exchange(ref max, product);
                    Interlocked.Exchange(ref first, firstPrime);
                    Interlocked.Exchange(ref second, secondPrime);
                }
            });

            return (max, first, second);
        }

        private static void Main(string[] args)
        {
            var sw = Stopwatch.StartNew();
            var primes = FindPrimes(10_000, 99_999);

            var (max, first, second) = FindLargestPalindrome(primes);

            Console.WriteLine($"MAX: {max} = {first} * {second} ({sw.ElapsedMilliseconds} ms)");
        }
    }
}
