'use client';

import React, { useMemo, useEffect, useRef } from 'react';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import remarkToc from 'remark-toc';
import rehypeHighlight from 'rehype-highlight';
import rehypeSlug from 'rehype-slug';
import { Highlight, themes } from 'prism-react-renderer';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { 
  Clock, 
  Calendar, 
  Hash, 
  ArrowUp,
  BookOpen,
  FileText,
  User
} from 'lucide-react';
import { cn } from '@/lib/utils';
import { useTheme } from 'next-themes';

interface DocumentationSection {
  id: string;
  title: string;
  content: string;
  type: string;
  order: number;
  metadata?: {
    wordCount?: number;
    readingTime?: number;
    lastModified?: string;
  };
}

interface Documentation {
  id: string;
  title: string;
  sections: DocumentationSection[];
  metadata?: {
    totalSections?: number;
    totalWords?: number;
    estimatedReadingTime?: number;
    lastGenerated?: string;
    generationDuration?: number;
  };
  generatedAt: string;
}

interface DocumentationContentProps {
  documentation: Documentation;
  selectedSection: string | null;
  searchTerm?: string;
  className?: string;
}

const highlightSearchTerm = (text: string, searchTerm: string): React.ReactNode => {
  if (!searchTerm.trim()) return text;
  
  const regex = new RegExp(`(${searchTerm.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')})`, 'gi');
  const parts = text.split(regex);
  
  return (
    <>
      {parts.map((part, index) => 
        regex.test(part) ? (
          <mark key={index} className="bg-yellow-200 dark:bg-yellow-700 px-1 rounded">
            {part}
          </mark>
        ) : (
          part
        )
      )}
    </>
  );
};

export const DocumentationContent: React.FC<DocumentationContentProps> = ({
  documentation,
  selectedSection,
  searchTerm = '',
  className = ''
}) => {
  const { theme, systemTheme } = useTheme();
  const contentRef = useRef<HTMLDivElement>(null);
  const isDarkMode = theme === 'dark' || (theme === 'system' && systemTheme === 'dark');
  
  // Determine which sections to display
  const sectionsToDisplay = useMemo(() => {
    if (selectedSection) {
      return documentation.sections.filter(section => section.id === selectedSection);
    }
    return documentation.sections.sort((a, b) => a.order - b.order);
  }, [documentation.sections, selectedSection]);

  // Custom markdown components
  const markdownComponents = useMemo(() => ({
    code({ node, inline, className, children, ...props }: any) {
      const match = /language-(\w+)/.exec(className || '');
      const language = match ? match[1] : '';
      
      return !inline && match ? (
        <Highlight
          theme={isDarkMode ? themes.vsDark : themes.github}
          code={String(children).replace(/\n$/, '')}
          language={language as any}
        >
          {({ className, style, tokens, getLineProps, getTokenProps }) => (
            <pre
              className={className}
              style={{
                ...style,
                margin: '1rem 0',
                borderRadius: '0.5rem',
                fontSize: '0.875rem',
                padding: '1rem',
                overflow: 'auto'
              }}
            >
              {tokens.map((line, i) => (
                <div key={i} {...getLineProps({ line })}>
                  {line.map((token, key) => (
                    <span key={key} {...getTokenProps({ token })} />
                  ))}
                </div>
              ))}
            </pre>
          )}
        </Highlight>
      ) : (
        <code className={cn('bg-gray-100 dark:bg-gray-800 px-1 py-0.5 rounded text-sm font-mono', className)} {...props}>
          {children}
        </code>
      );
    },
    h1: ({ children, ...props }: any) => (
      <h1 className="text-3xl font-bold text-gray-900 dark:text-white mt-8 mb-4 pb-2 border-b border-gray-200 dark:border-gray-700" {...props}>
        {searchTerm ? highlightSearchTerm(String(children), searchTerm) : children}
      </h1>
    ),
    h2: ({ children, ...props }: any) => (
      <h2 className="text-2xl font-semibold text-gray-900 dark:text-white mt-6 mb-3" {...props}>
        {searchTerm ? highlightSearchTerm(String(children), searchTerm) : children}
      </h2>
    ),
    h3: ({ children, ...props }: any) => (
      <h3 className="text-xl font-semibold text-gray-900 dark:text-white mt-5 mb-2" {...props}>
        {searchTerm ? highlightSearchTerm(String(children), searchTerm) : children}
      </h3>
    ),
    h4: ({ children, ...props }: any) => (
      <h4 className="text-lg font-semibold text-gray-900 dark:text-white mt-4 mb-2" {...props}>
        {searchTerm ? highlightSearchTerm(String(children), searchTerm) : children}
      </h4>
    ),
    h5: ({ children, ...props }: any) => (
      <h5 className="text-base font-semibold text-gray-900 dark:text-white mt-3 mb-2" {...props}>
        {searchTerm ? highlightSearchTerm(String(children), searchTerm) : children}
      </h5>
    ),
    h6: ({ children, ...props }: any) => (
      <h6 className="text-sm font-semibold text-gray-900 dark:text-white mt-3 mb-2" {...props}>
        {searchTerm ? highlightSearchTerm(String(children), searchTerm) : children}
      </h6>
    ),
    p: ({ children, ...props }: any) => (
      <p className="text-gray-700 dark:text-gray-300 mb-4 leading-relaxed" {...props}>
        {searchTerm ? highlightSearchTerm(String(children), searchTerm) : children}
      </p>
    ),
    blockquote: ({ children, ...props }: any) => (
      <blockquote className="border-l-4 border-blue-500 pl-4 py-2 my-4 bg-blue-50 dark:bg-blue-900/20 italic text-gray-700 dark:text-gray-300" {...props}>
        {children}
      </blockquote>
    ),
    table: ({ children, ...props }: any) => (
      <div className="overflow-x-auto my-4">
        <table className="w-full border-collapse border border-gray-300 dark:border-gray-600" {...props}>
          {children}
        </table>
      </div>
    ),
    th: ({ children, ...props }: any) => (
      <th className="border border-gray-300 dark:border-gray-600 bg-gray-100 dark:bg-gray-800 px-4 py-2 text-left font-semibold" {...props}>
        {children}
      </th>
    ),
    td: ({ children, ...props }: any) => (
      <td className="border border-gray-300 dark:border-gray-600 px-4 py-2" {...props}>
        {children}
      </td>
    ),
    ul: ({ children, ...props }: any) => (
      <ul className="list-disc list-inside mb-4 space-y-1" {...props}>
        {children}
      </ul>
    ),
    ol: ({ children, ...props }: any) => (
      <ol className="list-decimal list-inside mb-4 space-y-1" {...props}>
        {children}
      </ol>
    ),
    li: ({ children, ...props }: any) => (
      <li className="text-gray-700 dark:text-gray-300" {...props}>
        {children}
      </li>
    ),
    a: ({ children, href, ...props }: any) => (
      <a 
        className="text-blue-600 dark:text-blue-400 hover:underline" 
        href={href}
        target={href?.startsWith('http') ? '_blank' : undefined}
        rel={href?.startsWith('http') ? 'noopener noreferrer' : undefined}
        {...props}
      >
        {children}
      </a>
    ),
  }), [isDarkMode, searchTerm]);

  // Scroll to top when section changes
  useEffect(() => {
    if (contentRef.current) {
      contentRef.current.scrollTop = 0;
    }
  }, [selectedSection]);

  const scrollToTop = () => {
    if (contentRef.current) {
      contentRef.current.scrollTo({ top: 0, behavior: 'smooth' });
    }
  };

  if (sectionsToDisplay.length === 0) {
    return (
      <div className={cn('flex items-center justify-center h-full p-8', className)}>
        <div className="text-center">
          <BookOpen className="h-12 w-12 text-gray-400 mx-auto mb-4" />
          <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-2">
            {searchTerm ? 'No matching content' : 'No content available'}
          </h3>
          <p className="text-gray-500 dark:text-gray-400">
            {searchTerm 
              ? 'No sections match your search criteria.' 
              : 'This documentation appears to be empty.'}
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className={cn('relative h-full', className)}>
      <div 
        ref={contentRef}
        className="h-full overflow-auto"
      >
        <div className="max-w-4xl mx-auto p-6">
          {/* Documentation Header */}
          {!selectedSection && (
            <div className="mb-8">
              <h1 className="text-4xl font-bold text-gray-900 dark:text-white mb-4">
                {documentation.title}
              </h1>
              
              {/* Metadata */}
              {documentation.metadata && (
                <div className="flex flex-wrap items-center gap-4 text-sm text-gray-600 dark:text-gray-400 mb-6">
                  {documentation.metadata.totalSections && (
                    <div className="flex items-center space-x-1">
                      <FileText className="h-4 w-4" />
                      <span>{documentation.metadata.totalSections} sections</span>
                    </div>
                  )}
                  
                  {documentation.metadata.estimatedReadingTime && (
                    <div className="flex items-center space-x-1">
                      <Clock className="h-4 w-4" />
                      <span>{documentation.metadata.estimatedReadingTime} min read</span>
                    </div>
                  )}
                  
                  {documentation.metadata.lastGenerated && (
                    <div className="flex items-center space-x-1">
                      <Calendar className="h-4 w-4" />
                      <span>Updated {new Date(documentation.metadata.lastGenerated).toLocaleDateString()}</span>
                    </div>
                  )}
                </div>
              )}
              
              <div className="border-b border-gray-200 dark:border-gray-700 pb-6 mb-8" />
            </div>
          )}
          
          {/* Sections */}
          {sectionsToDisplay.map((section, index) => (
            <div key={section.id} className="mb-8">
              {/* Section Header */}
              {(sectionsToDisplay.length > 1 || selectedSection) && (
                <div className="flex items-center justify-between mb-4 pb-2 border-b border-gray-100 dark:border-gray-800">
                  <div className="flex items-center space-x-3">
                    <Badge variant="outline">
                      {section.type}
                    </Badge>
                    <h2 className="text-2xl font-semibold text-gray-900 dark:text-white">
                      {section.title}
                    </h2>
                  </div>
                  
                  {section.metadata && (
                    <div className="flex items-center space-x-3 text-sm text-gray-500">
                      {section.metadata.wordCount && (
                        <span>{section.metadata.wordCount} words</span>
                      )}
                      {section.metadata.readingTime && (
                        <div className="flex items-center space-x-1">
                          <Clock className="h-3 w-3" />
                          <span>{section.metadata.readingTime}m</span>
                        </div>
                      )}
                    </div>
                  )}
                </div>
              )}
              
              {/* Section Content */}
              <div className="prose prose-gray dark:prose-invert max-w-none">
                <ReactMarkdown
                  components={markdownComponents}
                  remarkPlugins={[remarkGfm, remarkToc]}
                  rehypePlugins={[rehypeHighlight, rehypeSlug]}
                >
                  {section.content}
                </ReactMarkdown>
              </div>
              
              {/* Section separator */}
              {index < sectionsToDisplay.length - 1 && (
                <div className="mt-8 pt-8 border-t border-gray-100 dark:border-gray-800" />
              )}
            </div>
          ))}
          
          {/* Footer */}
          <div className="mt-12 pt-6 border-t border-gray-200 dark:border-gray-700 text-center text-sm text-gray-500">
            <p>
              Generated on {new Date(documentation.generatedAt).toLocaleDateString()}
              {documentation.metadata?.generationDuration && (
                <span> in {documentation.metadata.generationDuration}ms</span>
              )}
            </p>
          </div>
        </div>
      </div>
      
      {/* Scroll to top button */}
      <Button
        variant="outline"
        size="sm"
        onClick={scrollToTop}
        className="fixed bottom-6 right-6 shadow-lg z-10"
      >
        <ArrowUp className="h-4 w-4" />
      </Button>
    </div>
  );
};

export default DocumentationContent;