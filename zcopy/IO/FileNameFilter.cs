using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace BananaHomie.ZCopy.IO
{
    public class FileNameFilter : ISearchFilter
    {
        public bool UseRegex { get; }
        public List<string> Filters { get; }
        public bool Exclude { get; set; }

        public FileNameFilter(bool useRegex, IEnumerable<string> filters, bool exclude = false)
        {
            UseRegex = useRegex;
            Filters = filters.ToList();
            Exclude = exclude;
        }

        public bool IsMatch(FileSystemInfo item)
        {
            return Filters.Select(f => WildcardToRegex(f, UseRegex)).All(filter => Exclude ? !filter.IsMatch(item.Name) : filter.IsMatch(item.Name));
        }

        private static Regex WildcardToRegex(string value, bool isAlreadyRegexString)
        {
            return isAlreadyRegexString ? new Regex(value) : new Regex(Regex.Escape(value).Replace("\\*", ".*").Replace("\\?", ".") + '$');
        }
    }
}