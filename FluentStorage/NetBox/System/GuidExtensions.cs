
namespace System {
    using NetBox;

	public static class GuidExtensions {
        public static string ToShortest(this Guid g) {
            return Ascii85.Instance.Encode(g.ToByteArray(), true);
        }
    }
}
