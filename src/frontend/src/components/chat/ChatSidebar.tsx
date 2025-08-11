'use client';

import React from 'react';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { MessageCircle, Plus, Clock, Folder, Loader2 } from 'lucide-react';
import { cn } from '@/lib/utils';

interface Conversation {
  id: string;
  title: string;
  repositoryId?: string;
  repositoryName?: string;
  messageCount: number;
  lastMessageAt: string;
  createdAt: string;
}

interface ChatSidebarProps {
  conversations: Conversation[];
  selectedConversation: string | null;
  repositoryId?: string;
  loading: boolean;
  onConversationSelect: (conversationId: string | null) => void;
  onStartNew: () => void;
  className?: string;
}

export const ChatSidebar: React.FC<ChatSidebarProps> = ({
  conversations,
  selectedConversation,
  repositoryId,
  loading,
  onConversationSelect,
  onStartNew,
  className = ''
}) => {
  const formatTimestamp = (timestamp: string) => {
    const date = new Date(timestamp);
    const now = new Date();
    const diffInHours = Math.floor((now.getTime() - date.getTime()) / (1000 * 60 * 60));
    
    if (diffInHours < 1) return 'Just now';
    if (diffInHours < 24) return `${diffInHours}h ago`;
    if (diffInHours < 24 * 7) return `${Math.floor(diffInHours / 24)}d ago`;
    return date.toLocaleDateString();
  };

  return (
    <div className={cn('flex flex-col h-full bg-gray-50 dark:bg-gray-900', className)}>
      {/* Header */}
      <div className="p-4 border-b border-gray-200 dark:border-gray-700">
        <Button
          onClick={onStartNew}
          className="w-full justify-start"
        >
          <Plus className="h-4 w-4 mr-2" />
          New Conversation
        </Button>
      </div>
      
      {/* Conversations List */}
      <div className="flex-1 overflow-y-auto">
        {loading ? (
          <div className="flex items-center justify-center p-8">
            <Loader2 className="h-6 w-6 animate-spin" />
          </div>
        ) : conversations.length === 0 ? (
          <div className="p-4 text-center text-gray-500 dark:text-gray-400">
            <MessageCircle className="h-8 w-8 mx-auto mb-2" />
            <p className="text-sm">No conversations yet</p>
          </div>
        ) : (
          <div className="p-2">
            {conversations.map((conversation) => (
              <Button
                key={conversation.id}
                variant={selectedConversation === conversation.id ? 'secondary' : 'ghost'}
                className={cn(
                  'w-full justify-start h-auto p-3 mb-2',
                  selectedConversation === conversation.id && 'bg-blue-50 dark:bg-blue-900/20'
                )}
                onClick={() => onConversationSelect(conversation.id)}
              >
                <div className="flex-1 text-left min-w-0">
                  <div className="flex items-center justify-between mb-1">
                    <h3 className="font-medium text-sm truncate pr-2">
                      {conversation.title}
                    </h3>
                    <div className="text-xs text-gray-500 dark:text-gray-400 flex-shrink-0">
                      {formatTimestamp(conversation.lastMessageAt)}
                    </div>
                  </div>
                  
                  <div className="flex items-center justify-between">
                    <div className="flex items-center space-x-2">
                      {conversation.repositoryName && (
                        <Badge variant="outline" className="text-xs">
                          <Folder className="h-3 w-3 mr-1" />
                          {conversation.repositoryName}
                        </Badge>
                      )}
                    </div>
                    
                    <div className="text-xs text-gray-500 dark:text-gray-400">
                      {conversation.messageCount} msgs
                    </div>
                  </div>
                </div>
              </Button>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default ChatSidebar;