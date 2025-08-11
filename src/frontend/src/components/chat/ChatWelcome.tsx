'use client';

import React from 'react';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { MessageCircle, Zap, Bot, Code, FileText, Search } from 'lucide-react';
import { cn } from '@/lib/utils';

interface ChatWelcomeProps {
  repositoryId?: string;
  suggestions: string[];
  onSuggestionClick: (suggestion: string) => void;
  className?: string;
}

export const ChatWelcome: React.FC<ChatWelcomeProps> = ({
  repositoryId,
  suggestions,
  onSuggestionClick,
  className = ''
}) => {
  const features = [
    {
      icon: Code,
      title: 'Code Analysis',
      description: 'Ask about code structure, patterns, and best practices'
    },
    {
      icon: FileText,
      title: 'Documentation',
      description: 'Generate and explore project documentation'
    },
    {
      icon: Search,
      title: 'Search',
      description: 'Find specific files, functions, or concepts'
    }
  ];

  return (
    <div className={cn('flex flex-col items-center justify-center h-full max-w-2xl mx-auto text-center p-8', className)}>
      <div className="mb-8">
        <div className="w-16 h-16 bg-blue-100 dark:bg-blue-900 rounded-full flex items-center justify-center mb-4 mx-auto">
          <Bot className="w-8 h-8 text-blue-600 dark:text-blue-400" />
        </div>
        <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-2">
          Welcome to Archie AI
        </h2>
        <p className="text-gray-600 dark:text-gray-400">
          {repositoryId
            ? 'Ask me anything about your repository. I can help with code analysis, documentation, and more.'
            : 'I can help you explore repositories, understand code, and generate documentation.'}
        </p>
        {repositoryId && (
          <Badge variant="outline" className="mt-2">
            Repository Context Active
          </Badge>
        )}
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-8 w-full">
        {features.map((feature) => (
          <div key={feature.title} className="p-4 border border-gray-200 dark:border-gray-700 rounded-lg">
            <feature.icon className="w-6 h-6 text-blue-600 dark:text-blue-400 mb-2" />
            <h3 className="font-semibold text-sm mb-1">{feature.title}</h3>
            <p className="text-xs text-gray-600 dark:text-gray-400">{feature.description}</p>
          </div>
        ))}
      </div>

      {suggestions.length > 0 && (
        <div className="w-full">
          <h3 className="text-sm font-medium text-gray-700 dark:text-gray-300 mb-3">
            Try asking:
          </h3>
          <div className="grid grid-cols-1 gap-2">
            {suggestions.slice(0, 4).map((suggestion, index) => (
              <Button
                key={index}
                variant="outline"
                size="sm"
                onClick={() => onSuggestionClick(suggestion)}
                className="text-left justify-start h-auto p-3"
              >
                <Zap className="w-4 h-4 mr-2 flex-shrink-0" />
                <span className="text-sm">{suggestion}</span>
              </Button>
            ))}
          </div>
        </div>
      )}
    </div>
  );
};

export default ChatWelcome;