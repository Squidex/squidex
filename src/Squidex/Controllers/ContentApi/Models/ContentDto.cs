// ==========================================================================
//  ContentDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.ComponentModel.DataAnnotations;
using Squidex.Core.Contents;
using Squidex.Infrastructure;

namespace Squidex.Controllers.ContentApi.Models
{
    public sealed class ContentDto
    {
        /// <summary>
        /// The if of the content element.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The user that has created the content element.
        /// </summary>
        [Required]
        public RefToken CreatedBy { get; set; }

        /// <summary>
        /// The user that has updated the content element.
        /// </summary>
        [Required]
        public RefToken LastModifiedBy { get; set; }

        /// <summary>
        /// The data of the content item.
        /// </summary>
        [Required]
        public object Data { get; set; }

        /// <summary>
        /// The date and time when the content element has been created.
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// The date and time when the content element has been modified last.
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// Indicates if the content element is publihed.
        /// </summary>
        public bool IsPublished { get; set; }
    }
}
