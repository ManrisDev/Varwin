using UnityEngine;

namespace Varwin
{
	public class DefaultHighlighter : MonoBehaviour, IHighlightAware
	{
		public IHighlightConfig HighlightConfig()
		{
			return HighlightAdapter.Instance.Configs.TouchHighlight;
		}
	}
}
