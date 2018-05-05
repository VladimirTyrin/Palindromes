using System;
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

        private static int[] FindPrimes(int start, int end, bool allowParallel)
        {
            return allowParallel
                ? FindPrimesParallel(start, end)
                : FindPrimesSequential(start, end);
        }

        private static int[] FindPrimesSequential(int start, int end)
        {
            var count = end - start;
            return Enumerable.Range(start, count).Where(IsPrime).ToArray();
        }

        private static int[] FindPrimesParallel(int start, int end)
        {
            var count = end - start;
            return Enumerable.Range(start, count).AsParallel().Where(IsPrime).ToArray();
        }

        private static bool IsPalindrome(long number)
        {
            // 10-digit palindromes will always have 11 as a dvivider
            if (number >= 1_000_000_000L)
                return false;

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

        private static (long max, long first, long second) FindLargestPalindrome(int[] primes, bool allowParallel)
        {
            return allowParallel
                ? FindLargestPalindromeParallel(primes)
                : FindLargestPalindromeSequential(primes);
        }

        private static (long max, long first, long second) FindLargestPalindromeParallel(int[] primes)
        {
            var max = 0L;
            long first = 0;
            long second = 0;

            Parallel.For(0, primes.Length, firstIndex =>
            {
                var firstPrime = primes[firstIndex];
                for (var index = firstIndex; index < primes.Length; index++)
                {
                    var secondPrime = primes[index];
                    if (secondPrime < firstPrime)
                        continue;

                    var product = (long)firstPrime * secondPrime;

                    if (product <= max)
                        continue;

                    if (!IsPalindrome(product))
                        continue;

                    Interlocked.Exchange(ref max, product);
                    Interlocked.Exchange(ref first, firstPrime);
                    Interlocked.Exchange(ref second, secondPrime);
                }
            });

            return (max, first, second);
        }

        private static (long max, long first, long second) FindLargestPalindromeSequential(int[] primes)
        {
            var max = 0L;
            long first = 0;
            long second = 0;

            for (var firstIndex = 0; firstIndex < primes.Length; firstIndex++)
            {
                var firstPrime = primes[firstIndex];
                for (var index = firstIndex; index < primes.Length; index++)
                {
                    var secondPrime = primes[index];
                    var product = (long) firstPrime * secondPrime;

                    if (product <= max)
                        continue;

                    if (!IsPalindrome(product))
                        continue;

                    max = product;
                    first = firstPrime;
                    second = secondPrime;
                }
            }

            return (max, first, second);
        }

        private static void Main(string[] args)
        {
            var allowParallel = true;
            var sw = Stopwatch.StartNew();
            var primes = FindPrimes(10_000, 99_999, allowParallel);

            var (max, first, second) = FindLargestPalindrome(primes, allowParallel);

            Console.WriteLine($"MAX: {max} = {first} * {second} ({sw.ElapsedMilliseconds} ms)");
        }
    }
}
