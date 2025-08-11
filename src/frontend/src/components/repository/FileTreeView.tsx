'use client';

import React from 'react';
import { ChevronRight, ChevronDown, Folder, FolderOpen, File } from 'lucide-react';
import { cn } from '@/lib/utils';

interface FileItem {
  name: string;
  path: string;
  size?: number;
  language?: string;
  lastModified?: string;
}

interface FolderItem {
  name: string;
  path: string;
  files?: FileItem[];
  subfolders?: FolderItem[];
}

interface FileStructure {
  folders?: FolderItem[];
  files?: FileItem[];
}

interface FileTreeViewProps {
  structure: FileStructure;
  expanded: Set<string>;
  selected: string | null;
  onToggle: (path: string) => void;
  onSelect: (path: string) => void;
  className?: string;
}

const getFileIcon = (fileName: string, language?: string) => {
  const ext = fileName.split('.').pop()?.toLowerCase();
  
  // Special file icons based on name or extension
  const iconMap: Record<string, string> = {
    'readme.md': '📖',
    'package.json': '📦',
    'tsconfig.json': '🔧',
    'dockerfile': '🐳',
    '.gitignore': '🚫',
    'license': '📄',
    'js': '🟨',
    'ts': '🔷',
    'tsx': '⚛️',
    'jsx': '⚛️',
    'css': '🎨',
    'scss': '🎨',
    'less': '🎨',
    'html': '🌐',
    'vue': '💚',
    'py': '🐍',
    'java': '☕',
    'c': '🔧',
    'cpp': '🔧',
    'cs': '💙',
    'go': '🐹',
    'rs': '🦀',
    'php': '🐘',
    'rb': '💎',
    'swift': '🔶',
    'kt': '🟣',
    'scala': '📐',
    'json': '📋',
    'xml': '📋',
    'yml': '📋',
    'yaml': '📋',
    'md': '📝',
    'txt': '📄',
    'pdf': '📕',
    'zip': '🗜️',
    'tar': '🗜️',
    'gz': '🗜️',
  };

  return iconMap[fileName.toLowerCase()] || iconMap[ext || ''] || '📄';
};

const FileTreeNode: React.FC<{
  item: FileItem | FolderItem;
  isFile: boolean;
  level: number;
  expanded: Set<string>;
  selected: string | null;
  onToggle: (path: string) => void;
  onSelect: (path: string) => void;
}> = ({ item, isFile, level, expanded, selected, onToggle, onSelect }) => {
  const isExpanded = expanded.has(item.path);
  const isSelected = selected === item.path;
  const indentStyle = { paddingLeft: `${level * 16 + 8}px` };

  if (isFile) {
    const fileItem = item as FileItem;
    return (
      <div
        className={cn(
          'flex items-center py-1 px-2 cursor-pointer hover:bg-gray-100 dark:hover:bg-gray-800 text-sm',
          isSelected && 'bg-blue-50 dark:bg-blue-900/20 text-blue-700 dark:text-blue-400'
        )}
        style={indentStyle}
        onClick={() => onSelect(fileItem.path)}
      >
        <span className="mr-2 text-base">{getFileIcon(fileItem.name, fileItem.language)}</span>
        <span className="truncate">{fileItem.name}</span>
        {fileItem.size && (
          <span className="ml-auto text-xs text-gray-500">
            {Math.round(fileItem.size / 1024)}KB
          </span>
        )}
      </div>
    );
  }

  const folderItem = item as FolderItem;
  const hasChildren = (folderItem.files && folderItem.files.length > 0) || 
                     (folderItem.subfolders && folderItem.subfolders.length > 0);

  return (
    <div>
      <div
        className={cn(
          'flex items-center py-1 px-2 cursor-pointer hover:bg-gray-100 dark:hover:bg-gray-800 text-sm',
          isSelected && 'bg-blue-50 dark:bg-blue-900/20 text-blue-700 dark:text-blue-400'
        )}
        style={indentStyle}
        onClick={() => hasChildren && onToggle(folderItem.path)}
      >
        {hasChildren && (
          <span className="mr-1">
            {isExpanded ? (
              <ChevronDown className="h-4 w-4" />
            ) : (
              <ChevronRight className="h-4 w-4" />
            )}
          </span>
        )}
        <span className="mr-2">
          {isExpanded ? (
            <FolderOpen className="h-4 w-4 text-blue-600" />
          ) : (
            <Folder className="h-4 w-4 text-gray-600" />
          )}
        </span>
        <span className="truncate font-medium">{folderItem.name}</span>
        {!hasChildren && (
          <span className="ml-auto text-xs text-gray-400">empty</span>
        )}
      </div>
      
      {isExpanded && hasChildren && (
        <div>
          {/* Render subfolders first */}
          {folderItem.subfolders?.map((subfolder) => (
            <FileTreeNode
              key={subfolder.path}
              item={subfolder}
              isFile={false}
              level={level + 1}
              expanded={expanded}
              selected={selected}
              onToggle={onToggle}
              onSelect={onSelect}
            />
          ))}
          
          {/* Then render files */}
          {folderItem.files?.map((file) => (
            <FileTreeNode
              key={file.path}
              item={file}
              isFile={true}
              level={level + 1}
              expanded={expanded}
              selected={selected}
              onToggle={onToggle}
              onSelect={onSelect}
            />
          ))}
        </div>
      )}
    </div>
  );
};

export const FileTreeView: React.FC<FileTreeViewProps> = ({
  structure,
  expanded,
  selected,
  onToggle,
  onSelect,
  className = ''
}) => {
  if (!structure) {
    return (
      <div className={cn('p-4 text-center text-gray-500', className)}>
        No file structure available
      </div>
    );
  }

  return (
    <div className={cn('py-2', className)}>
      {/* Render root folders */}
      {structure.folders?.map((folder) => (
        <FileTreeNode
          key={folder.path}
          item={folder}
          isFile={false}
          level={0}
          expanded={expanded}
          selected={selected}
          onToggle={onToggle}
          onSelect={onSelect}
        />
      ))}
      
      {/* Render root files */}
      {structure.files?.map((file) => (
        <FileTreeNode
          key={file.path}
          item={file}
          isFile={true}
          level={0}
          expanded={expanded}
          selected={selected}
          onToggle={onToggle}
          onSelect={onSelect}
        />
      ))}
      
      {/* Show message if no files/folders */}
      {(!structure.folders || structure.folders.length === 0) &&
       (!structure.files || structure.files.length === 0) && (
        <div className="p-4 text-center text-gray-500">
          No files found
        </div>
      )}
    </div>
  );
};

export default FileTreeView;