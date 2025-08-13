'use client';

import React, { useState, useMemo } from 'react';
import { useQuery } from '@apollo/client';
import { GET_REPOSITORY_DOCUMENTATION } from '@/graphql/queries';
import { DocumentationSection } from '@/types';
import { DocumentationTOC } from './DocumentationTOC';
import { DocumentationContent } from './DocumentationContent';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { 
  BookOpen, 
  Search, 
  Download, 
  FileText, 
  Clock, 
  Loader2, 
  AlertCircle, 
  Menu,
  X
} from 'lucide-react';
import { cn } from '@/lib/utils';
import { toast } from '@/hooks/use-toast';

interface DocumentationViewerProps {
  repositoryId: string;
  documentationId?: string;
  className?: string;
}

export const DocumentationViewer: React.FC<DocumentationViewerProps> = ({
  repositoryId,
  documentationId,
  className = ''
}) => {
  const [selectedSection, setSelectedSection] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [showTableOfContents, setShowTableOfContents] = useState(true);
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);

  // Query documentation
  const { data, loading, error, refetch } = useQuery(
    GET_REPOSITORY_DOCUMENTATION,
    {
      variables: { repositoryId, documentationId },
      errorPolicy: 'all'
    }
  );

  const documentation = data?.repository?.documentation;
  const repository = data?.repository;

  // Filter sections based on search term
  const filteredSections = useMemo(() => {
    if (!documentation?.sections || !searchTerm.trim()) {
      return documentation?.sections || [];
    }

    return documentation.sections.filter((section: DocumentationSection) =>
      section.title.toLowerCase().includes(searchTerm.toLowerCase()) ||
      section.content.toLowerCase().includes(searchTerm.toLowerCase())
    );
  }, [documentation?.sections, searchTerm]);

  const handleExportMarkdown = async () => {
    if (!documentation) return;

    try {
      const markdownContent = generateMarkdownExport(documentation);
      const blob = new Blob([markdownContent], { type: 'text/markdown' });
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `${repository?.name || 'documentation'}.md`;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      URL.revokeObjectURL(url);
      
      toast({
        title: 'Export successful',
        description: 'Documentation exported as Markdown file.',
      });
    } catch (error) {
      toast({
        title: 'Export failed',
        description: 'Failed to export documentation.',
        variant: 'destructive',
      });
    }
  };

  const handleExportPDF = async () => {
    if (!documentation) return;
    
    try {
      // For now, just show a message. PDF export would require additional libraries
      toast({
        title: 'PDF Export',
        description: 'PDF export feature coming soon. Use print-to-PDF from your browser for now.',
      });
      
      // Trigger browser print dialog
      window.print();
    } catch (error) {
      toast({
        title: 'Export failed',
        description: 'Failed to export documentation as PDF.',
        variant: 'destructive',
      });
    }
  };

  if (loading) {
    return (
      <div className={cn('flex items-center justify-center h-full p-8', className)}>
        <div className="text-center">
          <Loader2 className="h-8 w-8 animate-spin mx-auto mb-4" />
          <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-2">
            Loading documentation...
          </h3>
          <p className="text-gray-500 dark:text-gray-400">
            Please wait while we retrieve the documentation.
          </p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className={cn('p-6', className)}>
        <Alert variant="destructive">
          <AlertCircle className="h-4 w-4" />
          <AlertDescription>
            Failed to load documentation: {error.message}
            <Button 
              variant="outline" 
              size="sm" 
              onClick={() => refetch()}
              className="mt-2 ml-2"
            >
              Retry
            </Button>
          </AlertDescription>
        </Alert>
      </div>
    );
  }

  if (!documentation) {
    return (
      <div className={cn('flex items-center justify-center h-full p-8', className)}>
        <div className="text-center">
          <BookOpen className="h-12 w-12 text-gray-400 mx-auto mb-4" />
          <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-2">
            No documentation available
          </h3>
          <p className="text-gray-500 dark:text-gray-400">
            Documentation has not been generated for this repository yet.
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className={cn('flex h-full', className)}>
      {/* Table of Contents - Desktop */}
      {showTableOfContents && (
        <div className="hidden lg:flex lg:w-1/4 lg:min-w-[250px] border-r border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-900">
          <DocumentationTOC
            sections={documentation.sections}
            selectedSection={selectedSection}
            onSectionSelect={setSelectedSection}
            searchTerm={searchTerm}
          />
        </div>
      )}

      {/* Mobile TOC Overlay */}
      {isMobileMenuOpen && (
        <div className="fixed inset-0 z-50 lg:hidden">
          <div className="fixed inset-0 bg-black/50" onClick={() => setIsMobileMenuOpen(false)} />
          <div className="fixed left-0 top-0 bottom-0 w-3/4 max-w-sm bg-white dark:bg-gray-900 shadow-xl">
            <div className="flex items-center justify-between p-4 border-b border-gray-200 dark:border-gray-700">
              <h3 className="font-semibold">Table of Contents</h3>
              <Button 
                variant="ghost" 
                size="sm" 
                onClick={() => setIsMobileMenuOpen(false)}
              >
                <X className="h-4 w-4" />
              </Button>
            </div>
            <DocumentationTOC
              sections={documentation.sections}
              selectedSection={selectedSection}
              onSectionSelect={(section) => {
                setSelectedSection(section);
                setIsMobileMenuOpen(false);
              }}
              searchTerm={searchTerm}
            />
          </div>
        </div>
      )}

      {/* Main Content Area */}
      <div className="flex-1 flex flex-col bg-white dark:bg-gray-900 relative z-10">
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
            
            {/* Desktop TOC toggle */}
            <Button
              variant="ghost"
              size="sm"
              onClick={() => setShowTableOfContents(!showTableOfContents)}
              className="hidden lg:flex"
            >
              <Menu className="h-4 w-4" />
              Table of Contents
            </Button>
            
            {/* Search */}
            <div className="relative">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400" />
              <Input
                placeholder="Search documentation..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-10 w-64"
              />
            </div>
          </div>
          
          <div className="flex items-center space-x-2">
            {/* Documentation metadata */}
            <div className="hidden sm:flex items-center space-x-2 text-xs text-gray-500">
              {documentation.totalSections && (
                <Badge variant="outline">
                  {documentation.totalSections} sections
                </Badge>
              )}
              {documentation.estimatedReadingTime && (
                <div className="flex items-center space-x-1">
                  <Clock className="h-3 w-3" />
                  <span>{Math.round(documentation.estimatedReadingTime)} min read</span>
                </div>
              )}
            </div>
            
            {/* Export buttons */}
            <Button
              variant="outline"
              size="sm"
              onClick={handleExportMarkdown}
            >
              <FileText className="h-4 w-4 mr-1" />
              <span className="hidden sm:inline">Export MD</span>
            </Button>
            
            <Button
              variant="outline"
              size="sm"
              onClick={handleExportPDF}
            >
              <Download className="h-4 w-4 mr-1" />
              <span className="hidden sm:inline">Export PDF</span>
            </Button>
          </div>
        </div>
        
        {/* Content */}
        <div className="flex-1 overflow-auto bg-white dark:bg-gray-900">
          <DocumentationContent
            documentation={{
              ...documentation,
              sections: filteredSections
            }}
            selectedSection={selectedSection}
            searchTerm={searchTerm}
          />
        </div>
      </div>
    </div>
  );
};

// Helper function to generate markdown export
function generateMarkdownExport(documentation: any): string {
  let markdown = `# ${documentation.title}\n\n`;
  
  markdown += `> Generated: ${new Date(documentation.generatedAt).toLocaleDateString()}\n`;
  if (documentation.totalSections) {
    markdown += `> Sections: ${documentation.totalSections}\n`;
  }
  if (documentation.estimatedReadingTime) {
    markdown += `> Reading Time: ${Math.round(documentation.estimatedReadingTime)} minutes\n`;
  }
  markdown += `\n`;
  
  // Table of contents
  if (documentation.sections && documentation.sections.length > 0) {
    markdown += `## Table of Contents\n\n`;
    documentation.sections.forEach((section: any, index: number) => {
      const anchor = section.title.toLowerCase().replace(/\s+/g, '-').replace(/[^a-z0-9-]/g, '');
      markdown += `${index + 1}. [${section.title}](#${anchor})\n`;
    });
    markdown += `\n`;
  }
  
  // Content sections
  documentation.sections?.forEach((section: any) => {
    markdown += `## ${section.title}\n\n`;
    markdown += `${section.content}\n\n`;
  });
  
  return markdown;
}

export default DocumentationViewer;