using System;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;
using Newtonsoft.Json;
using Umbraco.Extensions;
using Microsoft.Extensions.Logging;

namespace Gibe.LinkPicker.PropertyConverters
{
	public class LinkPickerValueConverter : PropertyValueConverterBase
    {
        private readonly ILogger<LinkPickerValueConverter> _logger;
		private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;

		public LinkPickerValueConverter(ILogger<LinkPickerValueConverter> logger, IPublishedSnapshotAccessor publishedSnapshotAccessor)
        {
            _logger = logger;
            _publishedSnapshotAccessor = publishedSnapshotAccessor;
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
				return null;
			
			var sourceString = Convert.ToString(source);

			
			try
			{
				var linkPicker = JsonConvert.DeserializeObject<Models.LinkPicker>(sourceString);
				if (linkPicker != null && (linkPicker.Id > 0 || !string.IsNullOrWhiteSpace(linkPicker.Udi)))
                {
                    if (linkPicker.Udi != null)
                    {
                        var udi = linkPicker.Udi.TryConvertTo<Udi>();
                        if (udi.Success)
                        {
                            var content = _publishedSnapshotAccessor.GetRequiredPublishedSnapshot().Content.GetById(udi.Result);
                            linkPicker.Url = content?.Url() ?? linkPicker.Url;
						}
                    }
                    else
                    {
                        var content = _publishedSnapshotAccessor.GetRequiredPublishedSnapshot().Content
                            .GetById(linkPicker.Id);
                        linkPicker.Url = content?.Url() ?? linkPicker.Url;
					}
                   
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
