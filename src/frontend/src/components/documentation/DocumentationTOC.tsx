'use client';

import React, { useMemo } from 'react';
import { Button } from '@/components/ui/button';
import { 
  ChevronRight
} from 'lucide-react';
import { cn } from '@/lib/utils';

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

interface DocumentationTOCProps {
  sections: DocumentationSection[];
  selectedSection: string | null;
  onSectionSelect: (sectionId: string | null) => void;
  searchTerm?: string;
  className?: string;
}

// Professional typography-based section styling instead of emojis
const getSectionTypeStyle = (type: string): string => {
  switch (type.toLowerCase()) {
    case 'overview':
      return 'font-semibold text-gray-900 dark:text-white';
    case 'getting-started':
    case 'installation':
      return 'font-medium text-blue-700 dark:text-blue-300';
    case 'api':
    case 'reference':
      return 'font-normal text-gray-700 dark:text-gray-300';
    case 'guide':
    case 'tutorial':
    case 'examples':
      return 'font-normal text-green-700 dark:text-green-300';
    default:
      return 'font-normal text-gray-700 dark:text-gray-300';
  }
};

const cleanTitle = (title: string): string => {
  // Remove markdown header syntax (# ## ### etc)
  return title.replace(/^#+\s*/, '').trim();
};

const highlightSearchTerm = (text: string, searchTerm: string): React.ReactNode => {
  if (!searchTerm.trim()) return text;
  
  const regex = new RegExp(`(${searchTerm.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')})`, 'gi');
  const parts = text.split(regex);
  
  return (
    <>
      {parts.map((part, index) => 
        regex.test(part) ? (
          <span key={index} className="bg-blue-100 dark:bg-blue-900/50 text-blue-900 dark:text-blue-100 font-medium">
            {part}
          </span>
        ) : (
          part
        )
      )}
    </>
  );
};

export const DocumentationTOC: React.FC<DocumentationTOCProps> = ({
  sections,
  selectedSection,
  onSectionSelect,
  searchTerm = '',
  className = ''
}) => {
  // Filter sections based on search term
  const filteredSections = useMemo(() => {
    if (!searchTerm.trim()) return sections;
    
    return sections.filter(section =>
      section.title.toLowerCase().includes(searchTerm.toLowerCase()) ||
      section.content.toLowerCase().includes(searchTerm.toLowerCase())
    );
  }, [sections, searchTerm]);

  // Sort sections by order for clean navigation hierarchy
  const sortedSections = useMemo(() => {
    return [...filteredSections].sort((a, b) => a.order - b.order);
  }, [filteredSections]);

  if (sections.length === 0) {
    return (
      <div className={cn('p-4 text-center text-gray-500', className)}>
        <FileText className="h-8 w-8 mx-auto mb-2 text-gray-400" />
        <p>No sections available</p>
      </div>
    );
  }

  return (
    <div className={cn('flex flex-col h-full bg-white dark:bg-gray-900', className)}>
      {/* Clean, minimal header */}
      <div className="px-6 py-4 border-b border-gray-100 dark:border-gray-800">
        <h3 className="font-medium text-sm text-gray-600 dark:text-gray-400 uppercase tracking-wide">
          Contents
        </h3>
        
        {searchTerm && (
          <div className="mt-2 text-xs text-blue-600 dark:text-blue-400">
            Filtered by "{searchTerm}"
          </div>
        )}
      </div>

      {/* Professional navigation list */}
      <div className="flex-1 overflow-auto py-1">
        {sortedSections.length === 0 ? (
          <div className="px-6 py-8 text-center text-sm text-gray-500 dark:text-gray-400">
            {searchTerm ? 'No matching sections' : 'No sections available'}
          </div>
        ) : (
          <nav className="space-y-1 px-3">
            {/* Overview/All Sections - Clean, minimal */}
            <Button
              variant="ghost"
              className={cn(
                'w-full justify-start px-3 py-2 h-auto text-left font-normal rounded-md transition-colors',
                'hover:bg-gray-50 dark:hover:bg-gray-800/50',
                selectedSection === null && 'bg-blue-50 dark:bg-blue-900/30 text-blue-700 dark:text-blue-300 font-medium'
              )}
              onClick={() => onSectionSelect(null)}
            >
              <div className="flex items-center justify-between w-full">
                <span className="text-sm">
                  Overview
                </span>
                {selectedSection === null && (
                  <ChevronRight className="h-3 w-3 text-blue-600 dark:text-blue-400" />
                )}
              </div>
            </Button>
            
            {/* Individual sections - Clean typography-focused design */}
            {sortedSections.map((section) => {
              const isSelected = selectedSection === section.id;
              const cleanedTitle = cleanTitle(section.title);
              
              return (
                <Button
                  key={section.id}
                  variant="ghost"
                  className={cn(
                    'w-full justify-start px-3 py-2 h-auto text-left font-normal rounded-md transition-colors',
                    'hover:bg-gray-50 dark:hover:bg-gray-800/50',
                    isSelected && 'bg-blue-50 dark:bg-blue-900/30 text-blue-700 dark:text-blue-300 font-medium'
                  )}
                  onClick={() => onSectionSelect(section.id)}
                >
                  <div className="flex items-center justify-between w-full">
                    <div className="flex-1 min-w-0">
                      <div className={cn(
                        'text-sm truncate',
                        getSectionTypeStyle(section.type),
                        isSelected && 'text-blue-700 dark:text-blue-300'
                      )}>
                        {highlightSearchTerm(cleanedTitle, searchTerm)}
                      </div>
                    </div>
                    
                    {isSelected && (
                      <ChevronRight className="h-3 w-3 text-blue-600 dark:text-blue-400 flex-shrink-0 ml-2" />
                    )}
                  </div>
                </Button>
              );
            })}
          </nav>
        )}
      </div>
      
      {/* Minimal footer - only show when filtering */}
      {searchTerm && sortedSections.length > 0 && (
        <div className="px-6 py-2 border-t border-gray-100 dark:border-gray-800">
          <div className="text-xs text-gray-500 dark:text-gray-400">
            {sortedSections.length} of {sections.length} sections
          </div>
        </div>
      )}
    </div>
  );
};

export default DocumentationTOC;