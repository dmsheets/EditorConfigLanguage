﻿using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;

namespace EditorConfig
{
    internal class EditorConfigQuickInfo : IQuickInfoSource
    {
        private ITextBuffer _buffer;
        private EditorConfigDocument _document;

        public EditorConfigQuickInfo(ITextBuffer buffer)
        {
            _buffer = buffer;
            _document = EditorConfigDocument.FromTextBuffer(buffer);
        }

        public void AugmentQuickInfoSession(IQuickInfoSession session, IList<object> qiContent, out ITrackingSpan applicableToSpan)
        {
            applicableToSpan = null;

            SnapshotPoint? point = session.GetTriggerPoint(_buffer.CurrentSnapshot);

            if (session == null || qiContent == null || !point.HasValue || point.Value.Position >= point.Value.Snapshot.Length)
                return;

            var item = _document.ItemAtPosition(point.Value);

            if (item == null)
                return;

            foreach (var error in item.Errors)
            {
                qiContent.Add(error);
            }

            if (item.ItemType == ItemType.Property)
            {
                qiContent.Add(new TooltipControl(item));
            }
            else if (item.ItemType == ItemType.Severity && Constants.SeverityMonikers.ContainsKey(item.Text))
            {
                var desc = Constants.SeverityDescriptions[item.Text];
                var moniker = Constants.SeverityMonikers[item.Text];
                qiContent.Add(new TooltipControl(item.Text, desc, moniker));
            }
            else if (!string.IsNullOrEmpty(item.Description))
            {
                qiContent.Add(item.Description);
            }

            applicableToSpan = point.Value.Snapshot.CreateTrackingSpan(item.Span, SpanTrackingMode.EdgeNegative);
        }

        public void Dispose()
        {
        }
    }
}
