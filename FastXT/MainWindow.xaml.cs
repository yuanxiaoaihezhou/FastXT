using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;

namespace FastXT
{
    public partial class MainWindow : Window
    {
        private string? currentFilePath;
        private string? fullText;
        private List<Chapter> chapters = new List<Chapter>();
        private int currentChapterIndex = -1;
        private double currentFontSize = 16.0;

        public MainWindow()
        {
            InitializeComponent();
            RegisterKeyBindings();
        }

        private void RegisterKeyBindings()
        {
            // Ctrl+O: Open file
            var openCmd = new RoutedCommand();
            openCmd.InputGestures.Add(new KeyGesture(Key.O, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(openCmd, OpenFile_Click));

            // Ctrl++: Increase font size
            var increaseCmd = new RoutedCommand();
            increaseCmd.InputGestures.Add(new KeyGesture(Key.OemPlus, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(increaseCmd, IncreaseFontSize_Click));

            // Ctrl+-: Decrease font size
            var decreaseCmd = new RoutedCommand();
            decreaseCmd.InputGestures.Add(new KeyGesture(Key.OemMinus, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(decreaseCmd, DecreaseFontSize_Click));

            // Ctrl+Left: Previous chapter
            var prevCmd = new RoutedCommand();
            prevCmd.InputGestures.Add(new KeyGesture(Key.Left, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(prevCmd, PreviousChapter_Click));

            // Ctrl+Right: Next chapter
            var nextCmd = new RoutedCommand();
            nextCmd.InputGestures.Add(new KeyGesture(Key.Right, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(nextCmd, NextChapter_Click));
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "文本文件 (*.txt)|*.txt|所有文件 (*.*)|*.*",
                Title = "选择TXT文件"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                LoadFile(openFileDialog.FileName);
            }
        }

        private void LoadFile(string filePath)
        {
            try
            {
                StatusText.Text = "正在加载文件...";
                currentFilePath = filePath;

                // Fast file reading using encoding detection
                var startTime = DateTime.Now;
                fullText = ReadFileWithEncoding(filePath);
                var loadTime = (DateTime.Now - startTime).TotalMilliseconds;

                // Detect and split chapters
                DetectChapters();

                // Update UI
                if (chapters.Count > 0)
                {
                    ChapterListBox.ItemsSource = chapters;
                    ChapterListBox.DisplayMemberPath = "Title";
                    ChapterListBox.SelectedIndex = 0;
                }
                else
                {
                    // No chapters detected, show full text
                    TextDisplay.Text = fullText;
                }

                // Update status
                var fileInfo = new FileInfo(filePath);
                FileInfo.Text = $"{Path.GetFileName(filePath)} ({FormatFileSize(fileInfo.Length)})";
                StatusText.Text = $"文件加载完成 ({loadTime:F0}ms)";
                Title = $"FastXT - {Path.GetFileName(filePath)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载文件失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "加载失败";
            }
        }

        private string ReadFileWithEncoding(string filePath)
        {
            // Try to detect encoding by reading BOM
            byte[] buffer = new byte[4];
            using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                file.Read(buffer, 0, 4);
            }

            Encoding encoding = Encoding.UTF8;

            // Check for BOM
            if (buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF)
            {
                encoding = Encoding.UTF8;
            }
            else if (buffer[0] == 0xFF && buffer[1] == 0xFE)
            {
                encoding = Encoding.Unicode;
            }
            else if (buffer[0] == 0xFE && buffer[1] == 0xFF)
            {
                encoding = Encoding.BigEndianUnicode;
            }
            else
            {
                // Try to detect if it's UTF-8 without BOM or GB2312/GBK
                var testBytes = File.ReadAllBytes(filePath);
                if (IsLikelyUTF8(testBytes))
                {
                    encoding = new UTF8Encoding(false);
                }
                else
                {
                    // Default to GB2312/GBK for Chinese text files
                    try
                    {
                        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                        encoding = Encoding.GetEncoding("GB2312");
                    }
                    catch
                    {
                        encoding = Encoding.Default;
                    }
                }
            }

            // Fast reading using StreamReader
            using (var reader = new StreamReader(filePath, encoding))
            {
                return reader.ReadToEnd();
            }
        }

        private bool IsLikelyUTF8(byte[] bytes)
        {
            // Simple heuristic to check if bytes are valid UTF-8
            try
            {
                var decoder = Encoding.UTF8.GetDecoder();
                int charCount = decoder.GetCharCount(bytes, 0, Math.Min(bytes.Length, 1000));
                char[] chars = new char[charCount];
                decoder.GetChars(bytes, 0, Math.Min(bytes.Length, 1000), chars, 0);
                
                // Check for replacement characters which indicate invalid UTF-8
                return !chars.Contains('\uFFFD');
            }
            catch
            {
                return false;
            }
        }

        private void DetectChapters()
        {
            chapters.Clear();
            
            if (string.IsNullOrEmpty(fullText))
                return;

            // Chapter detection patterns (common Chinese novel chapter formats)
            var patterns = new[]
            {
                @"^第[零一二三四五六七八九十百千万0-9]+[章回][\s\:：].+",  // 第X章/回 标题
                @"^第[零一二三四五六七八九十百千万0-9]+[章回]\s*$",      // 第X章/回
                @"^Chapter\s+\d+.*",                                        // Chapter X
                @"^[0-9]+[\.\、]\s*.+",                                     // 1. 标题 or 1、标题
                @"^\【.+\】\s*$",                                           // 【标题】
                @"^====.+====\s*$",                                        // ==== 标题 ====
                @"^----.+----\s*$"                                         // ---- 标题 ----
            };

            var lines = fullText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var combinedPattern = string.Join("|", patterns.Select(p => $"({p})"));
            var regex = new Regex(combinedPattern, RegexOptions.Multiline);

            int position = 0;
            int lineNumber = 0;

            foreach (var line in lines)
            {
                if (regex.IsMatch(line.Trim()) && !string.IsNullOrWhiteSpace(line))
                {
                    chapters.Add(new Chapter
                    {
                        Title = line.Trim(),
                        StartPosition = position,
                        LineNumber = lineNumber
                    });
                }
                position += line.Length + Environment.NewLine.Length;
                lineNumber++;
            }

            // Set end positions
            for (int i = 0; i < chapters.Count - 1; i++)
            {
                chapters[i].EndPosition = chapters[i + 1].StartPosition;
            }
            if (chapters.Count > 0)
            {
                chapters[chapters.Count - 1].EndPosition = fullText.Length;
            }

            // If no chapters found, create a single chapter with all content
            if (chapters.Count == 0)
            {
                chapters.Add(new Chapter
                {
                    Title = "全文",
                    StartPosition = 0,
                    EndPosition = fullText.Length,
                    LineNumber = 0
                });
            }
        }

        private void ChapterListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ChapterListBox.SelectedIndex >= 0 && ChapterListBox.SelectedIndex < chapters.Count)
            {
                currentChapterIndex = ChapterListBox.SelectedIndex;
                DisplayCurrentChapter();
            }
        }

        private void DisplayCurrentChapter()
        {
            if (currentChapterIndex < 0 || currentChapterIndex >= chapters.Count || string.IsNullOrEmpty(fullText))
                return;

            var chapter = chapters[currentChapterIndex];
            var chapterText = fullText.Substring(chapter.StartPosition, chapter.EndPosition - chapter.StartPosition);
            TextDisplay.Text = chapterText;
            TextScrollViewer.ScrollToTop();

            ChapterInfo.Text = $"章节 {currentChapterIndex + 1}/{chapters.Count}";
        }

        private void PreviousChapter_Click(object sender, RoutedEventArgs e)
        {
            if (currentChapterIndex > 0)
            {
                ChapterListBox.SelectedIndex = currentChapterIndex - 1;
            }
        }

        private void NextChapter_Click(object sender, RoutedEventArgs e)
        {
            if (currentChapterIndex < chapters.Count - 1)
            {
                ChapterListBox.SelectedIndex = currentChapterIndex + 1;
            }
        }

        private void IncreaseFontSize_Click(object sender, RoutedEventArgs e)
        {
            currentFontSize = Math.Min(currentFontSize + 2, 48);
            TextDisplay.FontSize = currentFontSize;
        }

        private void DecreaseFontSize_Click(object sender, RoutedEventArgs e)
        {
            currentFontSize = Math.Max(currentFontSize - 2, 8);
            TextDisplay.FontSize = currentFontSize;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }

    public class Chapter
    {
        public string Title { get; set; } = string.Empty;
        public int StartPosition { get; set; }
        public int EndPosition { get; set; }
        public int LineNumber { get; set; }
    }
}
