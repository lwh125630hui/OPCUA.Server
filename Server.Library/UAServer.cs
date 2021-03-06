//<summary>
//  Title   : UAServer
//  System  : Microsoft Visual C# .NET 2008
//  $LastChangedDate:$
//  $Rev:$
//  $LastChangedBy:$
//  $URL:$
//  $Id:$
//
//  Copyright (C)2009, CAS LODZ POLAND.
//  TEL: +48 (42) 686 25 47
//  mailto://techsupp@cas.eu
//  http://www.cas.eu
//</summary>

using System;
using System.Collections.Generic;
using CAS.UA.Server.ServerConfiguration;
using Opc.Ua;
using Opc.Ua.Server;

namespace CAS.UA.Server.Library
{
  /// <summary>
  /// A class which implements an instance of a UA server.
  /// </summary>
  public partial class UAServer: StandardServer
  {
    #region private
    
    #region Overridden Methods
    /// <summary>
    /// Initializes the server before it starts up.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <remarks>
    /// This method is called before any startup processing occurs. The sub-class may update the
    /// configuration object or do any other application specific startup tasks.
    /// </remarks>
    protected override void OnServerStarting( ApplicationConfiguration configuration )
    {
      //TODO Comiterop  
      //Opc.Ua.Com.ComUtils.InitializeSecurity();
      CASConfiguration config = configuration as CASConfiguration;
      if ( config == null )
        throw new ArgumentNullException( typeof( CASConfiguration ).FullName );
      Utils.Trace( "The server is starting and the configuration is loaded." );
      try
      {
        m_DataBindings = new CAS.UA.Server.DataBindings.DataBindingsManager();
        m_DataBindings.Initialize( config );
        m_DataBindings.Start();
      }
      catch ( Exception ex )
      {
        TraceEvent.Tracer.TraceWarning( 51, this.GetType().Name + ".OnServerStarting", "Unable to do the bindings: " + ex.Message );
        Utils.Trace( "Unable to do the bindings: " + ex.Message );
      }
      base.OnServerStarting( configuration );
      // it is up to the application to decide how to validate user identity tokens.
      // this function creates validators for SAML and X509 identity tokens.
      CreateUserIdentityValidators( configuration );
    }
    /// <summary>
    /// Called after the server has been started.
    /// </summary>
    protected override void OnServerStarted( IServerInternal server )
    {
      base.OnServerStarted( server );
      // request notifications when the user identity is changed. all valid users are accepted by default.
      server.SessionManager.ImpersonateUser += new ImpersonateEventHandler( SessionManager_ImpersonateUser );
    }
    /// <summary>
    /// Cleans up before the server shuts down.
    /// </summary>
    /// <remarks>
    /// This method is called before any shutdown processing occurs.
    /// </remarks>
    protected override void OnServerStopping()
    {
      Console.WriteLine( "The Server is stopping." );
      if ( m_DataBindings != null )
        m_DataBindings.Stop();
      base.OnServerStopping();

#if INCLUDE_Sample
            CleanSampleModel();
#endif
    }
    /// <summary>
    /// Creates the node managers for the server.
    /// </summary>
    /// <remarks>
    /// This method allows the sub-class create any additional node managers which it uses. The SDK
    /// always creates a CoreNodeManager which handles the built-in nodes defined by the specification.
    /// Any additional NodeManagers are expected to handle application specific nodes.
    /// 
    /// Applications with small address spaces do not need to create their own NodeManagers and can add any
    /// application specific nodes to the CoreNodeManager. Applications should use custom NodeManagers when
    /// the structure of the address space is stored in another system or when the address space is too large
    /// to keep in memory.
    /// </remarks>
    protected override MasterNodeManager CreateMasterNodeManager( IServerInternal server, ApplicationConfiguration configuration )
    {
      Console.WriteLine( "Creating the Node Managers." );
      List<INodeManager> nodeManagers = new List<INodeManager>();
      // create the custom node managers.
      //nodeManagers.Add( new global::Boiler.BoilerNodeManager( server, configuration, m_DataBindings ) );
      nodeManagers.Add( new global::CAS.UA.Server.Library.Generic.GenericNodeManager( server, (CASConfiguration)configuration, m_DataBindings ) );
      // create master node manager.
      return new MasterNodeManager( server, configuration, null, nodeManagers.ToArray() );
    }
    /// <summary>
    /// Loads the non-configurable properties for the application.
    /// </summary>
    /// <returns>
    /// Returns the properties of the current server instance.
    /// </returns>
    /// <remarks>
    /// These properties are exposed by the server but cannot be changed by administrators.
    /// </remarks>
    protected override ServerProperties LoadServerProperties()
    {
      ServerProperties properties = new ServerProperties();
      properties.ManufacturerName = "CAS";
      properties.ProductName = "CAS CommServer UA Server";
      properties.ProductUri = "http://www.commsvr.com/Products/CommServerUA/tabid/376/language/en-US/Default.aspx";
      properties.SoftwareVersion = Utils.GetAssemblySoftwareVersion();
      properties.BuildNumber = Utils.GetAssemblyBuildNumber();
      properties.BuildDate = Utils.GetAssemblyTimestamp();

      // TBD - All applications have software certificates that need to added to the properties.

      // for (int ii = 0; ii < certificates.Count; ii++)
      // {
      //    properties.SoftwareCertificates.Add(certificates[ii]);
      // }
      return properties;
    }
    /// <summary>
    /// Initializes the address space after the NodeManagers have started.
    /// </summary>
    /// <remarks>
    /// This method can be used to create any initialization that requires access to node managers.
    /// </remarks>
    protected override void OnNodeManagerStarted( IServerInternal server )
    {
      Console.WriteLine( "The NodeManagers have started." );
      // allow base class processing to happen first.
      base.OnNodeManagerStarted( server );
      // adds the sample information models to the core node manager. 
    }
#if USER_AUTHENTICATION
    /// <summary>
    /// Creates the resource manager for the server.
    /// </summary>
    protected override ResourceManager CreateResourceManager( IServerInternal server, ApplicationConfiguration configuration )
    {
      ResourceManager resourceManager = new ResourceManager( server, configuration );

      // add some localized strings to the resource manager to demonstrate that localization occurs.
      resourceManager.Add( "InvalidPassword", "de-DE", "Das Passwort ist nicht gültig für Konto '{0}'." );
      resourceManager.Add( "InvalidPassword", "es-ES", "La contraseña no es válida para la cuenta de '{0}'." );

      resourceManager.Add( "UnexpectedUserTokenError", "fr-FR", "Une erreur inattendue s'est produite lors de la validation utilisateur." );
      resourceManager.Add( "UnexpectedUserTokenError", "de-DE", "Ein unerwarteter Fehler ist aufgetreten während des Anwenders." );

      return resourceManager;
    }
#endif
    #endregion

    private CAS.UA.Server.DataBindings.DataBindingsManager m_DataBindings;
    #endregion
  }
}
