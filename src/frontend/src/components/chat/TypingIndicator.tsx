'use client';

import React from 'react';
import { Avatar } from '@/components/ui/avatar';
import { Bot } from 'lucide-react';
import { cn } from '@/lib/utils';

export const TypingIndicator: React.FC<{ className?: string }> = ({ className }) => {
  return (
    <div className={cn('flex space-x-3 justify-start', className)}>
      <div className="flex-shrink-0">
        <Avatar className="h-8 w-8 bg-blue-100 dark:bg-blue-900">
          <Bot className="h-4 w-4 text-blue-600 dark:text-blue-400" />
        </Avatar>
      </div>
      
      <div className="max-w-3xl">
        <div className="bg-gray-100 dark:bg-gray-800 rounded-lg px-4 py-3">
          <div className="flex space-x-1">
            <div className="w-2 h-2 bg-gray-400 rounded-full animate-bounce" style={{ animationDelay: '0ms' }} />
            <div className="w-2 h-2 bg-gray-400 rounded-full animate-bounce" style={{ animationDelay: '150ms' }} />
            <div className="w-2 h-2 bg-gray-400 rounded-full animate-bounce" style={{ animationDelay: '300ms' }} />
          </div>
        </div>
        <div className="text-xs text-gray-500 dark:text-gray-400 mt-2 ml-1">
          Archie is thinking...
        </div>
      </div>
    </div>
  );
};

export default TypingIndicator;