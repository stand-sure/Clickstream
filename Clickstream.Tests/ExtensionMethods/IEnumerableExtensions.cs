//
//  IEnumerableExtensions.cs
//
//  Author:
//       C. J. Anderson <chris@standsure.io>
//
//  Copyright (c) 2016 2016
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
namespace Clickstream.Tests
{
  using System;
  using System.Collections.Generic;

  /// <summary>
  /// IEnumerable extensions.
  /// </summary>
  public static class IEnumerableExtensions
  {
    /// <summary>
    /// Provides a fluent foreach extention for <see cref="System.Collections.Generic.IEnumerable{T}"/>
    /// </summary>
    /// <param name="enumerable">The IEnumerable.</param>
    /// <param name="action">An Action.</param>
    /// <typeparam name="T">The type of the object(s) contained in the enumerable.</typeparam>
    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
    {
      foreach (T obj in enumerable)
      {
        action(obj);
      }
    }  }
}
