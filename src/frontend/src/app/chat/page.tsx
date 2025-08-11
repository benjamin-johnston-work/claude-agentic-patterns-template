'use client';

import React from 'react';
import { ChatInterface } from '@/components/chat/ChatInterface';
import { MainLayout } from '@/components/layout/MainLayout';

export default function ChatPage() {
  const handleRepositoryContext = (repositoryId: string) => {
    console.log('Repository context selected:', repositoryId);
    // Handle repository context change - could navigate to repository page
  };

  return (
    <MainLayout>
      <div className="h-full">
        <ChatInterface onRepositoryContext={handleRepositoryContext} />
      </div>
    </MainLayout>
  );
}