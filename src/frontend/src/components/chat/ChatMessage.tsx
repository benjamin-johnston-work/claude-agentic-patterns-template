'use client';

import React from 'react';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import { Highlight, themes } from 'prism-react-renderer';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Avatar } from '@/components/ui/avatar';
import { 
  Bot, 
  User, 
  Copy, 
  ExternalLink,
  Clock,
  Link,
  FileText
} from 'lucide-react';
import { cn } from '@/lib/utils';
import { useTheme } from 'next-themes';
import { toast } from '@/hooks/use-toast';

interface ChatMessage {
  id: string;
  content: string;
  sender: 'user' | 'assistant';
  timestamp: string;
  conversationId: string;
  metadata?: {
    type?: string;
    attachments?: Array<{
      type: string;
      url: string;
      title: string;
      description: string;
    }>;
  };
  repositoryContext?: string;
}

interface ChatMessageProps {
  message: ChatMessage;
  onRepositoryContext?: (repositoryId: string) => void;
  className?: string;
}

export const ChatMessage: React.FC<ChatMessageProps> = ({
  message,
  onRepositoryContext,
  className = ''
}) => {
  const { theme, systemTheme } = useTheme();
  const isDarkMode = theme === 'dark' || (theme === 'system' && systemTheme === 'dark');
  const isUser = message.sender === 'user';
  const isError = message.metadata?.type === 'error';
  
  const handleCopyMessage = async () => {
    try {
      await navigator.clipboard.writeText(message.content);
      toast({
        title: 'Copied to clipboard',
        description: 'Message content has been copied.',
      });
    } catch (error) {
      toast({
        title: 'Copy failed',
        description: 'Failed to copy message content.',
        variant: 'destructive',
      });
    }
  };
  
  const handleRepositoryLink = (repositoryId: string) => {
    onRepositoryContext?.(repositoryId);
  };
  
  const formatTimestamp = (timestamp: string) => {
    const date = new Date(timestamp);
    const now = new Date();
    const diffInMinutes = Math.floor((now.getTime() - date.getTime()) / (1000 * 60));
    
    if (diffInMinutes < 1) return 'Just now';
    if (diffInMinutes < 60) return `${diffInMinutes}m ago`;
    if (diffInMinutes < 24 * 60) return `${Math.floor(diffInMinutes / 60)}h ago`;
    return date.toLocaleDateString();
  };
  
  // Custom markdown components for AI messages
  const markdownComponents = {
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
                margin: '0.5rem 0',
                borderRadius: '0.375rem',
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
    a: ({ children, href, ...props }: any) => (
      <a 
        className="text-blue-600 dark:text-blue-400 hover:underline inline-flex items-center space-x-1" 
        href={href}
        target={href?.startsWith('http') ? '_blank' : undefined}
        rel={href?.startsWith('http') ? 'noopener noreferrer' : undefined}
        {...props}
      >
        <span>{children}</span>
        {href?.startsWith('http') && <ExternalLink className="h-3 w-3" />}
      </a>
    ),
    blockquote: ({ children, ...props }: any) => (
      <blockquote className="border-l-4 border-blue-500 pl-4 py-2 my-2 bg-blue-50 dark:bg-blue-900/20 italic" {...props}>
        {children}
      </blockquote>
    ),
    ul: ({ children, ...props }: any) => (
      <ul className="list-disc list-inside space-y-1 my-2" {...props}>
        {children}
      </ul>
    ),
    ol: ({ children, ...props }: any) => (
      <ol className="list-decimal list-inside space-y-1 my-2" {...props}>
        {children}
      </ol>
    ),
    p: ({ children, ...props }: any) => (
      <p className="mb-2 last:mb-0" {...props}>
        {children}
      </p>
    ),
    h3: ({ children, ...props }: any) => (
      <h3 className="text-lg font-semibold mt-4 mb-2" {...props}>
        {children}
      </h3>
    ),
    h4: ({ children, ...props }: any) => (
      <h4 className="text-base font-semibold mt-3 mb-2" {...props}>
        {children}
      </h4>
    ),
  };
  
  return (
    <div className={cn(
      'flex space-x-3',
      isUser ? 'justify-end' : 'justify-start',
      className
    )}>
      {!isUser && (
        <div className="flex-shrink-0">
          <Avatar className={cn(
            'h-8 w-8',
            isError ? 'bg-red-100 dark:bg-red-900' : 'bg-blue-100 dark:bg-blue-900'
          )}>
            <Bot className={cn(
              'h-4 w-4',
              isError ? 'text-red-600 dark:text-red-400' : 'text-blue-600 dark:text-blue-400'
            )} />
          </Avatar>
        </div>
      )}
      
      <div className={cn(
        'max-w-3xl',
        isUser ? 'order-first' : 'order-last'
      )}>
        {/* Message Bubble */}
        <div className={cn(
          'rounded-lg px-4 py-3',
          isUser 
            ? 'bg-blue-600 text-white' 
            : isError
              ? 'bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800'
              : 'bg-gray-100 dark:bg-gray-800 text-gray-900 dark:text-white',
        )}>
          {isUser ? (
            <p className="whitespace-pre-wrap">{message.content}</p>
          ) : (
            <div className={cn(
              'prose prose-sm max-w-none',
              isDarkMode ? 'prose-invert' : 'prose-gray',
              isError && 'text-red-700 dark:text-red-300'
            )}>
              <ReactMarkdown
                components={markdownComponents}
                remarkPlugins={[remarkGfm]}
              >
                {message.content}
              </ReactMarkdown>
            </div>
          )}
          
          {/* Attachments/Sources */}
          {message.metadata?.attachments && message.metadata.attachments.length > 0 && (
            <div className="mt-3 space-y-2">
              <div className="text-xs font-medium text-gray-500 dark:text-gray-400">
                Sources:
              </div>
              {message.metadata.attachments.map((attachment, index) => (
                <div key={index} className="bg-white dark:bg-gray-700 rounded border p-2">
                  <div className="flex items-start space-x-2">
                    <FileText className="h-4 w-4 text-gray-500 flex-shrink-0 mt-0.5" />
                    <div className="flex-1 min-w-0">
                      <div className="flex items-center space-x-2">
                        <span className="text-sm font-medium text-gray-900 dark:text-white truncate">
                          {attachment.title}
                        </span>
                        {attachment.url && (
                          <Button
                            variant="ghost"
                            size="sm"
                            className="h-5 w-5 p-0"
                            onClick={() => window.open(attachment.url, '_blank')}
                          >
                            <ExternalLink className="h-3 w-3" />
                          </Button>
                        )}
                      </div>
                      {attachment.description && (
                        <p className="text-xs text-gray-600 dark:text-gray-400 mt-1">
                          {attachment.description}
                        </p>
                      )}
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
        
        {/* Message Footer */}
        <div className="flex items-center justify-between mt-2">
          <div className="flex items-center space-x-4 text-xs text-gray-500 dark:text-gray-400">
            <div className="flex items-center space-x-1">
              <Clock className="h-3 w-3" />
              <span>{formatTimestamp(message.timestamp)}</span>
            </div>
            
            {message.repositoryContext && (
              <Button
                variant="ghost"
                size="sm"
                className="h-5 text-xs"
                onClick={() => handleRepositoryLink(message.repositoryContext!)}
              >
                <Link className="h-3 w-3 mr-1" />
                Repository Context
              </Button>
            )}
          </div>
          
          <div className="flex items-center space-x-1">
            <Button
              variant="ghost"
              size="sm"
              className="h-6 w-6 p-0"
              onClick={handleCopyMessage}
            >
              <Copy className="h-3 w-3" />
            </Button>
          </div>
        </div>
      </div>
      
      {isUser && (
        <div className="flex-shrink-0">
          <Avatar className="h-8 w-8 bg-gray-100 dark:bg-gray-800">
            <User className="h-4 w-4 text-gray-600 dark:text-gray-400" />
          </Avatar>
        </div>
      )}
    </div>
  );
};

export default ChatMessage;