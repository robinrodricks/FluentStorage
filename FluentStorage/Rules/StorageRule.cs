using FluentStorage.Model;

namespace FluentStorage.Rules;

/// <summary>
/// Base class used for all FluentStorage Rules. Extend this class to create custom rules.
/// You only need to provide an implementation for IsAllowed, and add any custom arguments that you require.
/// Originally from FluentFTP `FtpRule`.
/// </summary>
public class StorageRule {

	/// <summary>
	/// Rule object
	/// </summary>
	public StorageRule() {
	}

	/// <summary>
	/// Returns true if the object has passed this rules.
	/// </summary>
	public virtual bool IsAllowed(StoreObject result) {
		return true;
	}


}