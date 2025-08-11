'use client';

import React, { useState, useRef, useEffect, useMemo } from 'react';
import { useQuery, useMutation, useSubscription } from '@apollo/client';
import { 
  GET_CONVERSATIONS,
  GET_CONVERSATION 
} from '@/graphql/queries';
import {
  START_CONVERSATION,
  SEND_MESSAGE,
  PROCESS_QUERY
} from '@/graphql/mutations';
import { CHAT_MESSAGE_RECEIVED } from '@/graphql/subscriptions';
import { ChatMessage } from './ChatMessage';
import { ChatInput } from './ChatInput';
import { ChatWelcome } from './ChatWelcome';
import { ChatSidebar } from './ChatSidebar';
import { TypingIndicator } from './TypingIndicator';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { 
  MessageCircle, 
  Bot, 
  User,
  Menu,
  X,
  Plus,
  Settings,
  Archive,
  Loader2,
  AlertCircle,
  Zap
} from 'lucide-react';
import { cn } from '@/lib/utils';
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

interface Conversation {
  id: string;
  title: string;
  repositoryId?: string;
  repositoryName?: string;
  messageCount: number;
  lastMessageAt: string;
  createdAt: string;
  metadata?: {
    tags: string[];
    summary: string;
    participants: string[];
  };
}

interface ChatInterfaceProps {
  repositoryId?: string;
  onRepositoryContext?: (repositoryId: string) => void;
  className?: string;
}

const INITIAL_SUGGESTIONS = [
  "What is the architecture of this repository?",
  "Show me the main components and their relationships",
  "How do I get started with this codebase?",
  "What are the key patterns used in this project?",
  "Explain the data flow in this application"
];

const generateId = () => Math.random().toString(36).substr(2, 9);

export const ChatInterface: React.FC<ChatInterfaceProps> = ({
  repositoryId,
  onRepositoryContext,
  className = ''
}) => {
  const [selectedConversation, setSelectedConversation] = useState<string | null>(null);
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [inputValue, setInputValue] = useState('');
  const [isTyping, setIsTyping] = useState(false);
  const [suggestions, setSuggestions] = useState<string[]>(INITIAL_SUGGESTIONS);
  const [sidebarOpen, setSidebarOpen] = useState(true);
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
  const messagesEndRef = useRef<HTMLDivElement>(null);
  
  // Query conversations
  const { 
    data: conversationsData, 
    loading: conversationsLoading, 
    error: conversationsError,
    refetch: refetchConversations 
  } = useQuery(GET_CONVERSATIONS, {
    variables: { repositoryId, limit: 50 },
    errorPolicy: 'all'
  });
  
  // Query current conversation
  const { 
    data: conversationData, 
    loading: conversationLoading 
  } = useQuery(GET_CONVERSATION, {
    variables: { conversationId: selectedConversation },
    skip: !selectedConversation,
    errorPolicy: 'all'
  });
  
  // Start conversation mutation
  const [startConversation] = useMutation(START_CONVERSATION, {
    onCompleted: (data) => {
      if (data?.startConversation) {
        setSelectedConversation(data.startConversation.id);
        refetchConversations();
      }
    },
    onError: (error) => {
      toast({
        title: 'Failed to start conversation',
        description: error.message,
        variant: 'destructive',
      });
    }
  });
  
  // Send message mutation
  const [sendMessage] = useMutation(SEND_MESSAGE, {
    onError: (error) => {
      toast({
        title: 'Failed to send message',
        description: error.message,
        variant: 'destructive',
      });
      setIsTyping(false);
    }
  });
  
  // Process query mutation (AI response)
  const [processQuery] = useMutation(PROCESS_QUERY, {
    onCompleted: (data) => {
      if (data?.processQuery) {
        const aiMessage: ChatMessage = {
          id: generateId(),
          content: data.processQuery.response,
          sender: 'assistant',
          timestamp: new Date().toISOString(),
          conversationId: data.processQuery.conversationId || selectedConversation || '',
          metadata: {
            type: 'ai_response',
            attachments: data.processQuery.sources?.map((source: any) => ({
              type: 'source',
              url: source.url,
              title: source.title,
              description: source.excerpt
            })) || []
          }
        };
        
        setMessages(prev => [...prev, aiMessage]);
        
        // Update suggestions if provided
        if (data.processQuery.suggestions && data.processQuery.suggestions.length > 0) {
          setSuggestions(data.processQuery.suggestions);
        }
      }
      setIsTyping(false);
    },
    onError: (error) => {
      console.error('AI processing error:', error);
      const errorMessage: ChatMessage = {
        id: generateId(),
        content: `I'm sorry, I encountered an error while processing your request: ${error.message}`,
        sender: 'assistant',
        timestamp: new Date().toISOString(),
        conversationId: selectedConversation || '',
        metadata: { type: 'error' }
      };
      
      setMessages(prev => [...prev, errorMessage]);
      setIsTyping(false);
    }
  });
  
  // Subscribe to new messages
  useSubscription(CHAT_MESSAGE_RECEIVED, {
    variables: { conversationId: selectedConversation },
    skip: !selectedConversation,
    onSubscriptionData: ({ subscriptionData }) => {
      const newMessage = subscriptionData.data?.messageReceived;
      if (newMessage && newMessage.id) {
        // Only add if not already in messages
        setMessages(prev => {
          const exists = prev.some(msg => msg.id === newMessage.id);
          if (!exists) {
            return [...prev, newMessage];
          }
          return prev;
        });
        setIsTyping(false);
      }
    }
  });
  
  // Load messages when conversation changes
  useEffect(() => {
    if (conversationData?.conversation?.messages) {
      setMessages(conversationData.conversation.messages);
    } else if (selectedConversation === null) {
      setMessages([]);
    }
  }, [conversationData, selectedConversation]);
  
  // Auto-scroll to bottom
  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages, isTyping]);
  
  const conversations = conversationsData?.conversations || [];
  const currentConversation = conversations.find((c: Conversation) => c.id === selectedConversation);
  
  const handleStartNewConversation = async () => {
    try {
      const result = await startConversation({
        variables: {
          input: {
            title: 'New Conversation',
            repositoryId,
            context: repositoryId ? `Repository: ${repositoryId}` : undefined
          }
        }
      });
      
      if (result.data?.startConversation) {
        setMessages([]);
        setSuggestions(INITIAL_SUGGESTIONS);
      }
    } catch (error) {
      console.error('Failed to start conversation:', error);
    }
  };
  
  const handleSendMessage = async (message: string) => {
    if (!message.trim()) return;
    
    // Create user message
    const userMessage: ChatMessage = {
      id: generateId(),
      content: message,
      sender: 'user',
      timestamp: new Date().toISOString(),
      conversationId: selectedConversation || '',
      repositoryContext: repositoryId
    };
    
    setMessages(prev => [...prev, userMessage]);
    setInputValue('');
    setIsTyping(true);
    
    try {
      // If no conversation selected, start a new one
      let conversationId = selectedConversation;
      if (!conversationId) {
        const newConversation = await startConversation({
          variables: {
            input: {
              title: message.length > 50 ? message.substring(0, 50) + '...' : message,
              repositoryId,
              context: repositoryId ? `Repository: ${repositoryId}` : undefined
            }
          }
        });
        
        conversationId = newConversation.data?.startConversation?.id;
        if (!conversationId) {
          throw new Error('Failed to create conversation');
        }
      }
      
      // Send the message
      await sendMessage({
        variables: {
          input: {
            conversationId,
            content: message,
            repositoryContext: repositoryId
          }
        }
      });
      
      // Process with AI
      await processQuery({
        variables: {
          input: {
            query: message,
            conversationId,
            repositoryId,
            context: {
              previousMessages: messages.slice(-5).map(m => ({
                role: m.sender === 'user' ? 'user' : 'assistant',
                content: m.content
              })),
              repositoryContext: repositoryId
            }
          }
        }
      });
      
    } catch (error) {
      console.error('Failed to send message:', error);
      setIsTyping(false);
    }
  };
  
  const handleSuggestionClick = (suggestion: string) => {
    handleSendMessage(suggestion);
  };
  
  const handleConversationSelect = (conversationId: string | null) => {
    setSelectedConversation(conversationId);
    setIsMobileMenuOpen(false);
    if (conversationId === null) {
      setMessages([]);
      setSuggestions(INITIAL_SUGGESTIONS);
    }
  };
  
  if (conversationsError) {
    return (
      <div className={cn('p-6', className)}>
        <Alert variant="destructive">
          <AlertCircle className="h-4 w-4" />
          <AlertDescription>
            Failed to load conversations: {conversationsError.message}
          </AlertDescription>
        </Alert>
      </div>
    );
  }
  
  return (
    <div className={cn('flex h-full', className)}>
      {/* Sidebar - Desktop */}
      {sidebarOpen && (
        <div className="hidden lg:flex lg:w-80 lg:flex-shrink-0 border-r border-gray-200 dark:border-gray-700">
          <ChatSidebar
            conversations={conversations}
            selectedConversation={selectedConversation}
            repositoryId={repositoryId}
            loading={conversationsLoading}
            onConversationSelect={handleConversationSelect}
            onStartNew={handleStartNewConversation}
          />
        </div>
      )}
      
      {/* Mobile Sidebar Overlay */}
      {isMobileMenuOpen && (
        <div className="fixed inset-0 z-50 lg:hidden">
          <div className="fixed inset-0 bg-black/50" onClick={() => setIsMobileMenuOpen(false)} />
          <div className="fixed left-0 top-0 bottom-0 w-80 max-w-sm bg-white dark:bg-gray-900 shadow-xl">
            <div className="flex items-center justify-between p-4 border-b border-gray-200 dark:border-gray-700">
              <h3 className="font-semibold">Conversations</h3>
              <Button variant="ghost" size="sm" onClick={() => setIsMobileMenuOpen(false)}>
                <X className="h-4 w-4" />
              </Button>
            </div>
            <ChatSidebar
              conversations={conversations}
              selectedConversation={selectedConversation}
              repositoryId={repositoryId}
              loading={conversationsLoading}
              onConversationSelect={handleConversationSelect}
              onStartNew={handleStartNewConversation}
            />
          </div>
        </div>
      )}
      
      {/* Main Chat Area */}
      <div className="flex-1 flex flex-col">
        {/* Header */}
        <div className="flex items-center justify-between p-4 border-b border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800">
          <div className="flex items-center space-x-4">
            {/* Mobile menu button */}
            <Button
              variant="ghost"
              size="sm"
              onClick={() => setIsMobileMenuOpen(true)}
              className="lg:hidden"
            >
              <Menu className="h-4 w-4" />
            </Button>
            
            {/* Desktop sidebar toggle */}
            <Button
              variant="ghost"
              size="sm"
              onClick={() => setSidebarOpen(!sidebarOpen)}
              className="hidden lg:flex"
            >
              <Menu className="h-4 w-4" />
            </Button>
            
            <div className="flex items-center space-x-2">
              <MessageCircle className="h-5 w-5 text-blue-600" />
              <h1 className="text-lg font-semibold text-gray-900 dark:text-white">
                {currentConversation ? currentConversation.title : 'New Chat'}
              </h1>
              {repositoryId && (
                <Badge variant="outline">
                  {currentConversation?.repositoryName || 'Repository Context'}
                </Badge>
              )}
            </div>
          </div>
          
          <div className="flex items-center space-x-2">
            {currentConversation && (
              <div className="text-sm text-gray-500 dark:text-gray-400">
                {currentConversation.messageCount} messages
              </div>
            )}
            
            <Button variant="outline" size="sm" onClick={handleStartNewConversation}>
              <Plus className="h-4 w-4 mr-1" />
              New Chat
            </Button>
          </div>
        </div>
        
        {/* Messages Area */}
        <div className="flex-1 overflow-auto p-4">
          {conversationLoading ? (
            <div className="flex items-center justify-center h-full">
              <div className="text-center">
                <Loader2 className="h-8 w-8 animate-spin mx-auto mb-4" />
                <p>Loading conversation...</p>
              </div>
            </div>
          ) : messages.length === 0 ? (
            <ChatWelcome
              repositoryId={repositoryId}
              suggestions={suggestions}
              onSuggestionClick={handleSuggestionClick}
            />
          ) : (
            <div className="space-y-4">
              {messages.map((message) => (
                <ChatMessage
                  key={message.id}
                  message={message}
                  onRepositoryContext={onRepositoryContext}
                />
              ))}
              
              {isTyping && <TypingIndicator />}
              <div ref={messagesEndRef} />
            </div>
          )}
        </div>
        
        {/* Suggestions */}
        {suggestions.length > 0 && messages.length === 0 && (
          <div className="border-t border-gray-200 dark:border-gray-700 p-4">
            <div className="text-xs text-gray-500 dark:text-gray-400 mb-2">
              Suggested questions:
            </div>
            <div className="flex flex-wrap gap-2">
              {suggestions.map((suggestion, index) => (
                <Button
                  key={index}
                  variant="outline"
                  size="sm"
                  onClick={() => handleSuggestionClick(suggestion)}
                  className="text-xs"
                >
                  <Zap className="h-3 w-3 mr-1" />
                  {suggestion}
                </Button>
              ))}
            </div>
          </div>
        )}
        
        {/* Input */}
        <div className="border-t border-gray-200 dark:border-gray-700 p-4">
          <ChatInput
            value={inputValue}
            onChange={setInputValue}
            onSend={handleSendMessage}
            disabled={isTyping}
            repositoryId={repositoryId}
          />
        </div>
      </div>
    </div>
  );
};

export default ChatInterface;