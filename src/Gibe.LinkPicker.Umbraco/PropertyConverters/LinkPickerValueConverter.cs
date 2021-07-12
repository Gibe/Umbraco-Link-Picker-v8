using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Gibe.LinkPicker.PropertyConverters
{
	public class LinkPickerValueConverter : PropertyValueConverterBase
	{
		private readonly IUmbracoContextAccessor _contextAccessor;
		private readonly ILogger<LinkPickerValueConverter> _logger;

		public LinkPickerValueConverter(IUmbracoContextAccessor contextAccessor, ILogger<LinkPickerValueConverter> logger)
		{
			_contextAccessor = contextAccessor;
			_logger = logger;
		}

		/// <summary>
		/// Method to see if the current property type is of type
		/// LinkPicker editor.
		/// </summary>
		/// <param name="propertyType">The current property type.</param>
		/// <returns>True if the current property type of type LinkPicker editor.</returns>
		public override bool IsConverter(IPublishedPropertyType propertyType)
				=> propertyType.EditorAlias.InvariantEquals("Gibe.LinkPicker");

		/// <inheritdoc />
		public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
				=> typeof(Models.LinkPicker);

		/// <inheritdoc />
		public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
				=> PropertyCacheLevel.Element;

		/// <summary>
		/// Method to convert a property value to an instance of the LinkPicker class.
		/// </summary>
		/// <param name="owner"></param>
		/// <param name="propertyType">The current published property
		/// type to convert.</param>
		/// <param name="source">The original property data.</param>
		/// <param name="preview">True if in preview mode.</param>
		/// <returns>An instance of the LinkPicker class.</returns>
		public override object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object source, bool preview)
		{
			if (source == null)
			{
				return null;
			}

			var sourceString = Convert.ToString(source);

			using (var context = _contextAccessor.UmbracoContext)
			{
				try
				{
					var linkPicker = JsonConvert.DeserializeObject<Models.LinkPicker>(sourceString);
					if (linkPicker.Id > 0 || !string.IsNullOrWhiteSpace(linkPicker.Udi))
					{
						var content =
							linkPicker.Udi != null
								? context.Content.GetById(UdiParser.Parse(linkPicker.Udi))
								: context.Content.GetById(linkPicker.Id);

						linkPicker.Url = content?.Url() ?? linkPicker.Url;
					}

					return linkPicker;
				}
				catch (Exception ex)
				{

					_logger.LogError(ex.Message, ex);
					return null;
				}
			}
		}
	}
}
