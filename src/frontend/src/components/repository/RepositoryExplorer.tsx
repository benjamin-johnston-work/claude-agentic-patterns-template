'use client';

import React, { useState, useCallback } from 'react';
import { useQuery } from '@apollo/client';
import { GET_REPOSITORY_STRUCTURE, GET_FILE_CONTENT } from '@/graphql/queries';
import { FileTreeView } from './FileTreeView';
import { CodeViewer } from './CodeViewer';
import { Card } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { Loader2, FolderOpen, FileText, AlertCircle } from 'lucide-react';

interface RepositoryExplorerProps {
  repositoryId: string;
  onFileSelect?: (filePath: string) => void;
  onFolderToggle?: (folderPath: string) => void;
  className?: string;
}

export const RepositoryExplorer: React.FC<RepositoryExplorerProps> = ({
  repositoryId,
  onFileSelect,
  onFolderToggle,
  className = ''
}) => {
  const [selectedFile, setSelectedFile] = useState<string | null>(null);
  const [expandedFolders, setExpandedFolders] = useState<Set<string>>(new Set());

  // Query repository structure
  const { data: structureData, loading: structureLoading, error: structureError } = useQuery(
    GET_REPOSITORY_STRUCTURE,
    {
      variables: { repositoryId },
      errorPolicy: 'all'
    }
  );

  // Query file content when a file is selected
  const { data: fileData, loading: fileLoading, error: fileError } = useQuery(
    GET_FILE_CONTENT,
    {
      variables: {
        repositoryId,
        filePath: selectedFile
      },
      skip: !selectedFile,
      errorPolicy: 'all'
    }
  );

  const handleFileSelect = useCallback((filePath: string) => {
    setSelectedFile(filePath);
    onFileSelect?.(filePath);
  }, [onFileSelect]);

  const handleFolderToggle = useCallback((folderPath: string) => {
    setExpandedFolders(prev => {
      const newSet = new Set(prev);
      if (newSet.has(folderPath)) {
        newSet.delete(folderPath);
      } else {
        newSet.add(folderPath);
      }
      return newSet;
    });
    onFolderToggle?.(folderPath);
  }, [onFolderToggle]);

  const repository = structureData?.repository;
  const fileContent = fileData?.fileContent;

  if (structureError) {
    return (
      <div className={`p-6 ${className}`}>
        <Alert variant="destructive">
          <AlertCircle className="h-4 w-4" />
          <AlertDescription>
            Failed to load repository structure: {structureError.message}
          </AlertDescription>
        </Alert>
      </div>
    );
  }

  return (
    <div className={`flex h-full ${className}`}>
      {/* File Tree Panel */}
      <div className="w-1/3 min-w-[300px] border-r border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-900">
        <div className="p-4 border-b border-gray-200 dark:border-gray-700">
          <div className="flex items-center space-x-2">
            <FolderOpen className="h-5 w-5 text-blue-600" />
            <h3 className="font-semibold text-gray-900 dark:text-white">
              {repository?.name || 'Repository'}
            </h3>
          </div>
          {repository && (
            <div className="flex items-center space-x-2 mt-2">
              {repository.language && (
                <Badge variant="secondary">{repository.language}</Badge>
              )}
              <Badge variant="outline">
                {repository.statistics?.fileCount || 0} files
              </Badge>
            </div>
          )}
        </div>
        
        <div className="h-full overflow-auto">
          {structureLoading ? (
            <div className="flex items-center justify-center p-8">
              <Loader2 className="h-6 w-6 animate-spin" />
              <span className="ml-2">Loading repository structure...</span>
            </div>
          ) : repository?.fileStructure ? (
            <FileTreeView
              structure={repository.fileStructure}
              expanded={expandedFolders}
              selected={selectedFile}
              onToggle={handleFolderToggle}
              onSelect={handleFileSelect}
            />
          ) : (
            <div className="p-4 text-center text-gray-500">
              No files found in repository
            </div>
          )}
        </div>
      </div>

      {/* Code Viewer Panel */}
      <div className="flex-1 flex flex-col">
        {selectedFile ? (
          <>
            {/* File Header */}
            <div className="flex items-center justify-between p-4 border-b border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800">
              <div className="flex items-center space-x-2">
                <FileText className="h-4 w-4 text-gray-600" />
                <span className="font-mono text-sm text-gray-900 dark:text-white">
                  {selectedFile}
                </span>
                {fileContent && (
                  <Badge variant="outline">
                    {fileContent.language || 'text'}
                  </Badge>
                )}
              </div>
              {fileContent && (
                <div className="text-xs text-gray-500">
                  {fileContent.size && `${Math.round(fileContent.size / 1024)} KB`}
                  {fileContent.lastModified && (
                    <span className="ml-2">
                      Modified: {new Date(fileContent.lastModified).toLocaleDateString()}
                    </span>
                  )}
                </div>
              )}
            </div>

            {/* File Content */}
            <div className="flex-1 overflow-hidden">
              {fileError ? (
                <div className="p-6">
                  <Alert variant="destructive">
                    <AlertCircle className="h-4 w-4" />
                    <AlertDescription>
                      Failed to load file content: {fileError.message}
                    </AlertDescription>
                  </Alert>
                </div>
              ) : (
                <CodeViewer
                  content={fileContent?.content || ''}
                  language={fileContent?.language}
                  filePath={selectedFile}
                  loading={fileLoading}
                />
              )}
            </div>
          </>
        ) : (
          <div className="flex-1 flex items-center justify-center bg-gray-50 dark:bg-gray-900">
            <div className="text-center">
              <FileText className="h-12 w-12 text-gray-400 mx-auto mb-4" />
              <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-2">
                Select a file to view
              </h3>
              <p className="text-gray-500 dark:text-gray-400">
                Choose a file from the tree to see its contents
              </p>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default RepositoryExplorer;