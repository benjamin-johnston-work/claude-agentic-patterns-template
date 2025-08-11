'use client';

import React from 'react';
import { SearchInterface } from '@/components/search/SearchInterface';
import { MainLayout } from '@/components/layout/MainLayout';

export default function SearchPage() {
  const handleResultSelect = (result: any) => {
    console.log('Selected result:', result);
    // Handle result selection - could navigate to file viewer, etc.
  };

  return (
    <MainLayout>
      <div className="h-full">
        <SearchInterface onResultSelect={handleResultSelect} />
      </div>
    </MainLayout>
  );
}