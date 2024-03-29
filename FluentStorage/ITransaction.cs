﻿using System;
using System.Threading.Tasks;

namespace FluentStorage {
	/// <summary>
	/// Transaction abstraction
	/// </summary>
	public interface ITransaction : IDisposable {
		/// <summary>
		/// Commits the transaction
		/// </summary>
		/// <returns></returns>
		Task CommitAsync();
	}
}
