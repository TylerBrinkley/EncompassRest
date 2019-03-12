using System;
using System.ComponentModel;
using Newtonsoft.Json;

namespace EncompassRest.Loans
{
    /// <summary>
    /// FileAttachmentReference
    /// </summary>
    public sealed class FileAttachmentReference : EntityReference
    {
        private DirtyValue<string> _id;
        private DirtyValue<string> _refId;
        private DirtyValue<bool?> _isActive;
        private DirtyValue<string> _title;

        /// <summary>
        /// FileAttachmentReference Id
        /// </summary>
        public string Id { get => _id; set => SetField(ref _id, value); }

        /// <summary>
        /// FileAttachmentReference RefId
        /// </summary>
        public string RefId { get => _refId; set => SetField(ref _refId, value); }

        /// <summary>
        /// FileAttachmentReference IsActive
        /// </summary>
        public bool? IsActive { get => _isActive; set => SetField(ref _isActive, value); }

        /// <summary>
        /// FileAttachmentReference Title
        /// </summary>
        public string Title { get => _title; set => SetField(ref _title, value); }

        /// <summary>
        /// FileAttachmentReference constructor.
        /// </summary>
        /// <param name="entityId">The entity id.</param>
        /// <param name="entityType">The entity type.</param>
        public FileAttachmentReference(string entityId, EntityType entityType)
            : base(entityId, entityType)
        {
        }

        /// <summary>
        /// FileAttachmentReference constructor.
        /// </summary>
        /// <param name="entityId">The entity id.</param>
        /// <param name="entityType">The entity type.</param>
        public FileAttachmentReference(string entityId, string entityType)
            : base(entityId, entityType)
        {
        }

        /// <summary>
        /// FileAttachmentReference deserialization constructor.
        /// </summary>
        [Obsolete("Use another constructor instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [JsonConstructor]
        public FileAttachmentReference()
        {
        }
    }
}