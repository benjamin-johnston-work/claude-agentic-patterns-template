'use client';

import React, { useState, useMemo } from 'react';
import { Highlight, themes } from 'prism-react-renderer';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { useTheme } from 'next-themes';
import { 
  Copy, 
  Download, 
  Eye, 
  EyeOff, 
  Moon, 
  Sun, 
  ZoomIn, 
  ZoomOut,
  Loader2,
  FileText
} from 'lucide-react';
import { cn } from '@/lib/utils';
import { toast } from '@/hooks/use-toast';

interface CodeViewerProps {
  content: string;
  language?: string;
  filePath?: string;
  loading?: boolean;
  className?: string;
}

const detectLanguage = (filePath?: string, language?: string): string => {
  if (language) return language;
  if (!filePath) return 'text';
  
  const ext = filePath.split('.').pop()?.toLowerCase();
  const languageMap: Record<string, string> = {
    'js': 'javascript',
    'jsx': 'jsx',
    'ts': 'typescript',
    'tsx': 'tsx',
    'py': 'python',
    'java': 'java',
    'c': 'c',
    'cpp': 'cpp',
    'cs': 'csharp',
    'go': 'go',
    'rs': 'rust',
    'php': 'php',
    'rb': 'ruby',
    'swift': 'swift',
    'kt': 'kotlin',
    'scala': 'scala',
    'html': 'html',
    'css': 'css',
    'scss': 'scss',
    'less': 'less',
    'json': 'json',
    'xml': 'xml',
    'yml': 'yaml',
    'yaml': 'yaml',
    'md': 'markdown',
    'sql': 'sql',
    'sh': 'bash',
    'bash': 'bash',
    'ps1': 'powershell',
    'dockerfile': 'docker',
    'vue': 'vue',
    'svelte': 'svelte'
  };
  
  return languageMap[ext || ''] || 'text';
};

const formatFileSize = (size: number): string => {
  if (size < 1024) return `${size} B`;
  if (size < 1024 * 1024) return `${Math.round(size / 1024)} KB`;
  return `${Math.round(size / (1024 * 1024))} MB`;
};

export const CodeViewer: React.FC<CodeViewerProps> = ({
  content,
  language,
  filePath,
  loading = false,
  className = ''
}) => {
  const { theme, systemTheme } = useTheme();
  const [showLineNumbers, setShowLineNumbers] = useState(true);
  const [fontSize, setFontSize] = useState(14);
  const [wordWrap, setWordWrap] = useState(false);
  
  const detectedLanguage = detectLanguage(filePath, language);
  const isDarkMode = theme === 'dark' || (theme === 'system' && systemTheme === 'dark');
  
  const currentTheme = isDarkMode ? themes.vsDark : themes.github;

  const handleCopyToClipboard = async () => {
    try {
      await navigator.clipboard.writeText(content);
      toast({
        title: 'Copied to clipboard',
        description: 'File content has been copied to your clipboard.',
      });
    } catch (error) {
      toast({
        title: 'Copy failed',
        description: 'Failed to copy content to clipboard.',
        variant: 'destructive',
      });
    }
  };

  const handleDownloadFile = () => {
    const blob = new Blob([content], { type: 'text/plain' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filePath ? filePath.split('/').pop() || 'file.txt' : 'file.txt';
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
    
    toast({
      title: 'Download started',
      description: 'File download has been initiated.',
    });
  };

  const increaseFontSize = () => setFontSize(prev => Math.min(prev + 2, 24));
  const decreaseFontSize = () => setFontSize(prev => Math.max(prev - 2, 10));

  if (loading) {
    return (
      <div className={cn('flex items-center justify-center h-full bg-gray-50 dark:bg-gray-900', className)}>
        <div className="flex items-center space-x-2">
          <Loader2 className="h-6 w-6 animate-spin" />
          <span>Loading file content...</span>
        </div>
      </div>
    );
  }

  if (!content) {
    return (
      <div className={cn('flex items-center justify-center h-full bg-gray-50 dark:bg-gray-900', className)}>
        <div className="text-center">
          <FileText className="h-12 w-12 text-gray-400 mx-auto mb-4" />
          <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-2">
            No content to display
          </h3>
          <p className="text-gray-500 dark:text-gray-400">
            This file appears to be empty or could not be loaded.
          </p>
        </div>
      </div>
    );
  }

  const lineCount = content.split('\n').length;
  const characterCount = content.length;

  return (
    <div className={cn('flex flex-col h-full', className)}>
      {/* Toolbar */}
      <div className="flex items-center justify-between p-3 bg-gray-50 dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700">
        <div className="flex items-center space-x-2">
          <Badge variant="outline" className="font-mono text-xs">
            {detectedLanguage}
          </Badge>
          <span className="text-xs text-gray-600 dark:text-gray-400">
            {lineCount.toLocaleString()} lines • {formatFileSize(characterCount)}
          </span>
        </div>
        
        <div className="flex items-center space-x-1">
          <Button
            variant="ghost"
            size="sm"
            onClick={() => setShowLineNumbers(!showLineNumbers)}
            title={showLineNumbers ? 'Hide line numbers' : 'Show line numbers'}
          >
            {showLineNumbers ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
          </Button>
          
          <Button
            variant="ghost"
            size="sm"
            onClick={() => setWordWrap(!wordWrap)}
            title={wordWrap ? 'Disable word wrap' : 'Enable word wrap'}
          >
            <FileText className={cn('h-4 w-4', wordWrap && 'text-blue-600')} />
          </Button>
          
          <div className="flex items-center border-l border-gray-300 dark:border-gray-600 pl-2 ml-2">
            <Button
              variant="ghost"
              size="sm"
              onClick={decreaseFontSize}
              disabled={fontSize <= 10}
              title="Decrease font size"
            >
              <ZoomOut className="h-4 w-4" />
            </Button>
            
            <span className="mx-2 text-xs text-gray-600 dark:text-gray-400 min-w-[30px] text-center">
              {fontSize}px
            </span>
            
            <Button
              variant="ghost"
              size="sm"
              onClick={increaseFontSize}
              disabled={fontSize >= 24}
              title="Increase font size"
            >
              <ZoomIn className="h-4 w-4" />
            </Button>
          </div>
          
          <div className="flex items-center border-l border-gray-300 dark:border-gray-600 pl-2 ml-2">
            <Button
              variant="ghost"
              size="sm"
              onClick={handleCopyToClipboard}
              title="Copy to clipboard"
            >
              <Copy className="h-4 w-4" />
            </Button>
            
            <Button
              variant="ghost"
              size="sm"
              onClick={handleDownloadFile}
              title="Download file"
            >
              <Download className="h-4 w-4" />
            </Button>
          </div>
        </div>
      </div>

      {/* Code Content */}
      <div className="flex-1 overflow-auto">
        <Highlight
          theme={currentTheme}
          code={content}
          language={detectedLanguage as any}
        >
          {({ className, style, tokens, getLineProps, getTokenProps }) => (
            <pre
              className={className}
              style={{
                ...style,
                margin: 0,
                fontSize: `${fontSize}px`,
                background: isDarkMode ? '#1e1e1e' : '#ffffff',
                height: '100%',
                padding: '1rem',
                whiteSpace: wordWrap ? 'pre-wrap' : 'pre',
                overflowX: wordWrap ? 'hidden' : 'auto',
                fontFamily: 'ui-monospace, SFMono-Regular, "SF Mono", Consolas, "Liberation Mono", Menlo, monospace'
              }}
            >
              {tokens.map((line, i) => (
                <div key={i} {...getLineProps({ line })}>
                  {showLineNumbers && (
                    <span
                      style={{
                        display: 'inline-block',
                        width: '3em',
                        textAlign: 'right',
                        marginRight: '1em',
                        opacity: 0.5,
                        userSelect: 'none'
                      }}
                    >
                      {i + 1}
                    </span>
                  )}
                  {line.map((token, key) => (
                    <span key={key} {...getTokenProps({ token })} />
                  ))}
                </div>
              ))}
            </pre>
          )}
        </Highlight>
      </div>
    </div>
  );
};

export default CodeViewer;