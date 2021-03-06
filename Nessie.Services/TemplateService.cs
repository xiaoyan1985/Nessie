﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nessie.Services
{
    public class TemplateService
    {
        private const string TemplateWildcardCategory = "";
        public const string TemplatePrefix = "_template_";

        public IReadOnlyList<FileLocation> GetApplicableTemplates(
            IReadOnlyCollection<FileLocation> allTemplates,
            FileLocation file)
        {
            if (file.Extension != ".md")
            {
                return Array.Empty<FileLocation>();
            }

            var templatesWithCategory = allTemplates
                .Select(template => (template, category: template.FileNameWithoutExtension.Replace(TemplatePrefix, "")))
                .ToArray();

            // if this file has a category, but there's no template in the same directory, don't return any templates.
            // this prevents a item/_item_foo.md being rendered with a top-level template in a parent directory.
            if (!templatesWithCategory.Any() || !FileHasAdjacentTemplate(file, templatesWithCategory))
            {
                return Array.Empty<FileLocation>();
            }

            var applicableTemplates = FilterTemplates(file, templatesWithCategory);

            return applicableTemplates;
        }

        private static IReadOnlyList<FileLocation> FilterTemplates(
            FileLocation file,
            IReadOnlyCollection<(FileLocation template, string category)> templatesWithCategory)
        {
            // expand /foo/bar/baz into a list of all parent directories e.g. '', '/foo', '/foo/bar', '/foo/bar/baz' 
            var driveLetter = file.Directory.Length >= 2 && file.Directory[1] == ':'
                ? file.Directory.Substring(0, 2)
                : string.Empty;

            var directoryParts = file.Directory
                .Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries)
                .Select(part => part == driveLetter ? string.Empty : part);

            var directories = directoryParts
                .Prepend(string.Empty)
                .Scan(string.Empty, Path.Combine) // don't use Path.Combine because it doesn't handle combining with drive letters
                .Select(path => driveLetter == string.Empty ? path : driveLetter + Path.DirectorySeparatorChar + path)
                .ToArray();

            // find all templates in the parent directories that either apply to all files, or the category of this file.
            var applicableTemplates = templatesWithCategory
                .Where(template =>
                    FileIsInCategory(file, template.category)
                    && directories.Contains(template.template.Directory))
                .OrderByDescending(template => template.template.FullyQualifiedName.Length)
                .Select(template => template.template)
                .ToList();

            return applicableTemplates;
        }

        private static bool FileIsInCategory(FileLocation file, string category)
        {
            return category == TemplateWildcardCategory || file.Category == category;
        }

        // adjacent is defined as in the same directory.
        private static bool FileHasAdjacentTemplate(FileLocation file, (FileLocation template, string category)[] templatesWithCategory)
        {
            return file.Category == TemplateWildcardCategory || templatesWithCategory.Last().category == file.Category;
        }
    }
}
