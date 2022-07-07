/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace SocialEdgeSDK.Server.DataService
{
    public interface ICollection<T>
    {
        ///<summary>Returns an estimate of the number of documents in the collection.</summary>      
        ///<returns>An estimate of the number of documents in the collection.</returns>        
        long EstimatedDocumentCount{ get;}
        ///<summary>Counts the number of documents in the collection. For a fast estimate of the
        //     total documents in a collection use EstimatedDocumentCount</summary>      
        ///<returns>The number of documents in the collection.</returns>
        long DocumentCount{ get;}
        
        ///<summary>Finds the document matching the id.</summary>      
        ///<param name="id">id of the document to match. </param>
        ///<returns>Bson document.</returns>
        Task<T> FindOneById(string id);
        ///<summary>Finds the document matching the id.</summary>      
        ///<param name="id">id of the document to match. </param>
        ///<param name="projection">Projection definition </param>
        ///<returns>Projected bson document.</returns>        
        Task<U> FindOneById<U>(string id, ProjectionDefinition<T> projection);
        ///<summary>Finds a document matching the the value of the property. Incase of multiple matches, 
        ///fetches the first match</summary>      
        ///<param name="prop">The property to match</param>
        ///<param name="val">The value of the property to match</param>
        ///<returns>Bson document.</returns>          
        Task<T> FindOne<S>(string prop, S val);
        ///<summary>Finds a document matching the the value of the property. Incase of multiple matches,
        ///fetches the first match</summary>      
        ///<param name="prop">The property to match</param>
        ///<param name="val">The value of the property to match</param>
        ///<param name="projection">Projection definition</param>
        ///<returns>Projected bson document.</returns>            
        Task<T> FindOne<S>(string prop, S val, ProjectionDefinition<T> projection);
        ///<summary>Finds document matching the filter.</summary>      
        ///<param name="filter">FilterDefinition</param>
        ///<returns>Bson document.</returns>   
        Task<T> FindOne(FilterDefinition<T> filter);
        ///<summary>Finds document matching the filter.</summary>      
        ///<param name="filter">Filter definition</param>
        ///<param name="projection">Projection definition</param>
        ///<returns>Projected bson document.</returns>           
        Task<P> FindOne<P>(FilterDefinition<T> filter, ProjectionDefinition<T> projection);
        ///<summary>Finds documents matching the the value of the property.</summary>      
        ///<param name="prop">The property to match</param>
        ///<param name="val">The value of the property to match</param>
        ///<returns>List of Bson documents</returns>  
        Task<List<T>> Find<S>(string prop, S val);
        ///<summary>Finds documents matching the the value of the property.</summary>      
        ///<param name="prop">The property to match</param>
        ///<param name="val">The value of the property to match</param>
        ///<param name="projection">Projection definition</param>
        ///<returns>List of projected bson documents</returns>          
        Task<List<T>> Find<S>(string prop,S val, ProjectionDefinition<T> projection);
        ///<summary>Finds documents matching the filter.</summary>      
        ///<param name="filter">FilterDefinition</param>
        ///<returns>List of Bson documents</returns>   
        Task<List<T>> Find(FilterDefinition<T> Filter);
        ///<summary>Finds documents matching the filter.</summary>      
        ///<param name="filter">FilterDefinition</param>
        ///<param name="projection">Projection definition</param>
        ///<returns>List of projected bson documents</returns>          
        Task<List<T>> Find(FilterDefinition<T> Filter, ProjectionDefinition<T> projection);
        ///<summary>Updates the matching document</summary>      
        ///<param name="id">Id of the document to update</param>
        ///<param name="prop">The property to update</param>
        ///<param name="val">New value of prop</param>
        ///<param name="upsert">Upsert</param>
        ///<returns>UpdateResult</returns>           

        Task<UpdateResult> ReplaceOneById(string id, T val, bool upsert=false);
        Task<UpdateResult> ReplaceOneById(ObjectId id, T val, bool upsert=false);
        Task<T> IncAll(string prop, int incBy, bool upsert = false);

        Task<UpdateResult> UpdateOneById<S>(string id, string prop, S val, bool upsert=false);
        ///<summary>Updates the matching document</summary>      
        ///<param name="UpdateDefinition">Update definition</param>
        ///<returns>UpdateResult</returns>           
        Task<UpdateResult> UpdateOneById(string id, UpdateDefinition<T> UpdateDefinition, bool upsert=false);
        ///<summary>Updates the first matching document</summary>      
        ///<param name="filterProp">Property to match</param>
        ///<param name="filterVal">Value of property in filterProp to match</param>
        ///<param name="updateProp">The property to update</param>
        ///<param name="updateVal">New value of updateProp</param>
        ///<param name="upsert">Upsert</param>
        ///<returns>UpdateResult</returns>             
        Task<UpdateResult> UpdateOne<S,U>(string filterProp, S filterVal, string updateProp, U updateVal, bool upsert=false);
       ///<summary>Updates the first matching document</summary>      
        ///<param name="filterProp">Property to match</param>
        ///<param name="filterVal">Value of property in filterProp to match</param>
        ///<param name="updateDefinition">Update definition</param>
        ///<param name="upsert">Upsert</param>
        ///<returns>UpdateResult</returns>            
        Task<UpdateResult> UpdateOne<S>(string filterProp, S filterVal, UpdateDefinition<T> updateDefinition, bool upsert=false);
       ///<summary>Updates the first matching document</summary>      
        ///<param name="filter">Filter definition</param>
        ///<param name="updateDefinition">Update definition</param>
        ///<param name="upsert">Upsert</param>
        ///<returns>UpdateResult</returns>   
        Task<UpdateResult> UpdateOne(FilterDefinition<T> filter,UpdateDefinition<T> updateDefinition, bool upsert=false);
        ///<summary>Updates the first matching document</summary>      
        ///<param name="prop">Property to match</param>
        ///<param name="val">Value of property in filterProp to match</param>
        ///<param name="updateDefinition">Update definition</param>
        ///<param name="upsert">Upsert</param>
        ///<returns>UpdateResult</returns>            
        Task<UpdateResult> UpdateMany<S>(string prop,S val, UpdateDefinition<T> updateDefinition, bool upsert=false);
        ///<summary>Updates all matching documents</summary>      
        ///<param name="filterProp">Property to match</param>
        ///<param name="filterVal">Value of property in filterProp to match</param>
        ///<param name="updateProp">The property to update</param>
        ///<param name="updateVal">New value of updateProp</param>
        ///<param name="upsert">Upsert</param>
        ///<returns>UpdateResult</returns>               
        Task<UpdateResult> UpdateMany<S,U>(string filterProp, S filterVal, string updateProp, U updateVal, bool upsert=false);
        ///<summary>Updates all matching documents</summary>      
        ///<param name="filter">Filter definition</param>
        ///<param name="updateDefinition">Update definition</param>
        ///<param name="upsert">Upsert</param>
        ///<returns>UpdateResult</returns>          
        Task<UpdateResult> UpdateMany(FilterDefinition<T> filter,UpdateDefinition<T> updateDefinition, bool upsert=false);
        ///<summary>Removes the document that matches the if</summary>      
        ///<param name="id">id of the document to remove</param>
        ///<returns>Bool indicating operation was successful or not</returns> 
        Task<bool> RemoveOneById(string id);
        ///<summary>Removes the document that matches the value of given property</summary>      
        ///<param name="prop">Property to match</param>
        ///<param name="val">Value of the property to match</param>
        ///<returns>Bool indicating operation was successful or not</returns>         
        Task<bool> RemoveOne<S>(string prop, S val);
        ///<summary>Removes the document that matches the filter</summary>      
        ///<param name="filter">Filter definition</param>
        ///<returns>Bool indicating operation was successful or not</returns>  
        Task<bool> RemoveOne(FilterDefinition<T> filter);
        ///<summary>Removes documents which match the value of given property</summary>      
        ///<param name="prop">Property to match</param>
        ///<param name="val">Value of the property to match</param>
        ///<returns>Bool indicating operation was successful or not</returns>           
        Task<long> RemoveMany<S>(string prop, S val);
        ///<summary>Removes documents which match the filter</summary>      
        ///<param name="filter">Filter definition</param>
        ///<returns>Bool indicating operation was successful or not</returns>          
        Task<long> RemoveMany(FilterDefinition<T> Filter);
        ///<summary>Inserts a document</summary>      
        ///<param name="document">Bson document to insert</param>
        ///<returns>Bool indicating operation was successful or not</returns>         
        Task<bool> InsertOne(T document);
        ///<summary>Inserts multiple document</summary>      
        ///<param name="document">List of Bson documents to insert</param>
        ///<returns>Bool indicating operation was successful or not</returns>              
        Task<bool> InsertMany(List<T> documents);
        // Task<bool> Save(BsonDocument document);
        ///<summary>Creates an index</summary>      
        ///<param name="key">the field to create index on</param>
        ///<param name="direction">1 for ascending, -1 for descending</param>
        ///<param name="unique">Should the index be unique or not</param>
        ///<param name="name">Name of the index</param>
        ///<param name="TTL">Time To Live for the index</param>        
        ///<returns>Name of the index that was created.</returns>           
        Task<string> CreateIndex(string key, int direction=1,  bool unique=false, 
                                string name=null, TimeSpan? TTL=null);
        ///<summary>Creates a multifield index</summary>      
        ///<param name="key1">the first field to create index on</param>
        ///<param name="direction1">1 for ascending, -1 for descending</param>
        ///<param name="key2">the secondd to create index on</param>
        ///<param name="direction2">1 for ascending, -1 for descending</param>        
        ///<param name="unique">Should the index be unique or not</param>
        ///<param name="name">Name of the index</param>
        ///<param name="TTL">Time To Live for the index</param>        
        ///<returns>Name of the index that was created.</returns>  
        Task<string> CreateIndex(string key1, string key2,  int direction1=1, int direction2=1,
                                bool unique=false,string name=null, TimeSpan? TTL=null);




    }
}